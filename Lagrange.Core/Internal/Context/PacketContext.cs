using System.Collections.Concurrent;
using System.Security.Cryptography;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Services;

namespace Lagrange.Core.Internal.Context;

internal class PacketContext
{
    private readonly ConcurrentDictionary<int, SsoPacketValueTaskSource> _pendingTasks = new();

    private readonly BotKeystore _keystore;
    private readonly SsoPacker _ssoPacker;
    private readonly ServicePacker _servicePacker;
    private readonly MsfNgPacker _msfNgPacker;

    internal readonly BotSignProvider SignProvider;

    private readonly BotContext _context;

    public PacketContext(BotContext context)
    {
        _context = context;
        _keystore = context.Keystore;
        _ssoPacker = new SsoPacker(context);
        _servicePacker = new ServicePacker(context);
        _msfNgPacker = new MsfNgPacker(context);

        SignProvider = context.Config.SignProvider ?? context.Config.Protocol switch
        {
            Protocols.Linux or Protocols.Windows or Protocols.MacOs => new DefaultBotSignProvider(),
            Protocols.AndroidPhone or Protocols.AndroidPad => new DefaultAndroidBotSignProvider(),
            _ => throw new ArgumentOutOfRangeException(nameof(context.Config.Protocol))
        };

        SignProvider.Context = context; // Initialize the sign provider with the context
    }

    public ValueTask<BotSsoPacket> SendPacket(BotSsoPacket packet, ServiceAttribute options)
    {
        var tcs = new SsoPacketValueTaskSource();
        _pendingTasks.TryAdd(packet.Sequence, tcs);

        Task.Run(async () => // Schedule the task to the ThreadPool
        {
            try
            {
                ReadOnlyMemory<byte> frame;

                bool skipSign = Environment.GetEnvironmentVariable("CHARON_SKIP_SIGN") == "1";
                Console.Error.WriteLine($"[TRACE] SendPacket enter cmd={packet.Command} type={options.RequestType} skipSign={skipSign}");
                if (_context.Config.UseMsfNgTransport)
                {
                    frame = options.RequestType switch
                    {
                        RequestType.D2Auth => _msfNgPacker.BuildProtocol12(packet, options),
                        RequestType.Simple => _msfNgPacker.BuildProtocol13(packet, options),
                        _ => throw new InvalidOperationException($"Unknown RequestType: {options.RequestType}")
                    };
                }
                else
                {
                    switch (options.RequestType)
                    {
                        case RequestType.D2Auth:
                        {
                            if (SignProvider.IsWhiteListCommand(packet.Command) && !skipSign)
                            {
                                var secInfo = await SignProvider.GetSecSign(_keystore.Uin, packet.Command, packet.Sequence, packet.Data);
                                Console.Error.WriteLine("[TRACE] sign done");
                                var sso = _ssoPacker.BuildProtocol12(packet, secInfo);
                                frame = _servicePacker.BuildProtocol12(sso, options);
                            }
                            else
                            {
                                Console.Error.WriteLine("[TRACE] building sso (no sign) v12");
                                var sso = _ssoPacker.BuildProtocol12(packet, null);
                                Console.Error.WriteLine("[TRACE] sso built v12");
                                frame = _servicePacker.BuildProtocol12(sso, options);
                                Console.Error.WriteLine("[TRACE] service frame built v12");
                            }

                            break;
                        }
                        case RequestType.Simple:
                        {
                            if (!skipSign && SignProvider.IsWhiteListCommand(packet.Command))
                            {
                                Console.Error.WriteLine($"[TRACE] GetSecSign calling for {packet.Command}");
                                var secInfo = await SignProvider.GetSecSign(_keystore.Uin, packet.Command, packet.Sequence, packet.Data);
                                Console.Error.WriteLine($"[TRACE] GetSecSign returned for {packet.Command}");
                                var sso = _ssoPacker.BuildProtocol13(packet, secInfo);
                                frame = _servicePacker.BuildProtocol13(packet, sso, options);
                            }
                            else
                            {
                                Console.Error.WriteLine("[TRACE] building sso (no sign) v13");
                                var sso = _ssoPacker.BuildProtocol13(packet, null);
                                frame = _servicePacker.BuildProtocol13(packet, sso, options);
                            }
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException($"Unknown RequestType: {options.RequestType}");
                        }
                    }
                }

                Console.Error.WriteLine($"[TRACE] socket send {frame.Length}B cmd={packet.Command}");
                await _context.SocketContext.Send(frame);
                Console.Error.WriteLine($"[TRACE] socket sent cmd={packet.Command}");
            }
            catch (Exception e)
            {
                if (_pendingTasks.TryRemove(packet.Sequence, out var tcs))
                {
                    tcs.SetException(e);
                }
            }
        });

        return new ValueTask<BotSsoPacket>(tcs, 0);
    }

    public async Task<bool> EstablishMsfNgSessionAsync()
    {
        var request = MsfNgKeyExchange.BuildRequest(out var ephemeral, MsfNgKeyExchange.Command, ReadOnlyMemory<byte>.Empty);
        var sso = new BotSsoPacket(MsfNgKeyExchange.Command, request);
        var options = new ServiceAttribute(MsfNgKeyExchange.Command, RequestType.Simple, EncryptType.NoEncrypt);

        var response = await SendPacket(sso, options);
        if (response.Data.IsEmpty) return false;

        var keys = MsfNgKeyExchange.ParseResponse(response.Data.Span, ephemeral);
        if (keys is null) return false;

        _msfNgPacker.SessionKey = keys.Secret1.Length == 16 ? keys.Secret1 : keys.Secret2.Length == 16 ? keys.Secret2 : MD5.HashData(keys.Secret1);
        return true;
    }

    public void DispatchPacket(ReadOnlySpan<byte> buffer)
    {
        if (_context.Config.UseMsfNgTransport)
        {
            DispatchMsfNgPacket(buffer);
            return;
        }

        var service = _servicePacker.Parse(buffer);
        var sso = _ssoPacker.Parse(service);

        if (_pendingTasks.TryRemove(sso.Sequence, out var tcs))
        {
            if (sso is { RetCode: not 0, Extra: var extra })
            {
                string msg = $"Packet '{sso.Command}' returns {sso.RetCode} with seq: {sso.Sequence}, extra: {extra}";
                tcs.SetException(new InvalidOperationException(msg));
            }
            else
            {
                tcs.SetResult(sso);
            }
        }
        else
        {
            Task.Run(() => _context.EventContext.HandleServerPacket(sso));
        }
    }

    private void DispatchMsfNgPacket(ReadOnlySpan<byte> buffer)
    {
        var packet = _msfNgPacker.Parse(buffer);
        if (packet is null) return; // channel level frame e.g. heartbeat pong

        SsoPacketValueTaskSource? tcs;
        if (!_pendingTasks.TryRemove(packet.HeadSequence, out tcs) &&
            !_pendingTasks.TryRemove(packet.BasicSequence, out tcs))
        {
            var sso = packet.RetCode == 0
                ? new BotSsoPacket(packet.Command, packet.Payload, packet.HeadSequence)
                : new BotSsoPacket(packet.Command, packet.HeadSequence, packet.RetCode, packet.Extra);
            Task.Run(() => _context.EventContext.HandleServerPacket(sso));
            return;
        }

        if (packet is { RetCode: not 0, Extra: var extra })
        {
            string msg = $"Packet '{packet.Command}' returns {packet.RetCode} with seq: {packet.HeadSequence}, extra: {extra}";
            tcs.SetException(new InvalidOperationException(msg));
        }
        else
        {
            tcs.SetResult(new BotSsoPacket(packet.Command, packet.Payload, packet.HeadSequence));
        }
    }
}
