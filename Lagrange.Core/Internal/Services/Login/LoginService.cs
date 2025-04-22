using System.Diagnostics;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Packets.Login;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Internal.Services.Login;

[EventSubscribe<LoginEventReq>(Protocols.PC)]
[Service("wtlogin.login", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class LoginService : BaseService<LoginEventReq, LoginEventResp>
{
    private Lazy<WtLogin> _packet = new();

    protected override ValueTask<ReadOnlyMemory<byte>> Build(LoginEventReq input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));
        
        return input.Cmd switch
        {
            LoginEventReq.Command.Tgtgt =>  new ValueTask<ReadOnlyMemory<byte>>(_packet.Value.BuildOicq09()),
            _ => throw new ArgumentOutOfRangeException(nameof(input), $"Unknown command: {input.Cmd}")
        };
    }

    protected override ValueTask<LoginEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        var payload = _packet.Value.Parse(input.Span, out ushort command);
        var reader = new BinaryPacket(payload);
        Debug.Assert(command == 0x810);
        
        ushort internalCmd = reader.Read<ushort>();
        byte state = reader.Read<byte>();
        var tlvs = ProtocolHelper.TlvUnPack(ref reader);

        if (tlvs.TryGetValue(0x146, out var error))
        {
            var errorReader = new BinaryPacket(error.AsSpan());
            uint _ = errorReader.Read<uint>(); // error code
            string errorTitle = errorReader.ReadString(Prefix.Int16 | Prefix.LengthOnly);
            string errorMessage = errorReader.ReadString(Prefix.Int16 | Prefix.LengthOnly);
            
            return new ValueTask<LoginEventResp?>(new LoginEventResp(state, (errorTitle, errorMessage)));
        }

        if (tlvs.TryGetValue(0x119, out var tgtgt))
        {
            TeaProvider.Decrypt(tgtgt, tgtgt, context.Keystore.WLoginSigs.TgtgtKey);
            var tlv119 = TeaProvider.CreateDecryptSpan(tgtgt);
            var tlv119Reader = new BinaryPacket(tlv119);
            var tlvCollection = ProtocolHelper.TlvUnPack(ref tlv119Reader);
            
            return new ValueTask<LoginEventResp?>(new LoginEventResp(state, tlvCollection));
        }

        return new ValueTask<LoginEventResp?>(new LoginEventResp(state, new Dictionary<ushort, byte[]>()));
    }
}

[EventSubscribe<LoginEventReq>(Protocols.Android)]
[Service("wtlogin.login", RequestType.D2Auth, EncryptType.EncryptEmpty)]
internal class LoginServiceAndroid : BaseService<LoginEventReq, LoginEventResp>
{
    private Lazy<WtLogin> _packet = new();

    protected override ValueTask<ReadOnlyMemory<byte>> Build(LoginEventReq input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        return base.Build(input, context);
    }

    protected override ValueTask<LoginEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        if (!_packet.IsValueCreated) _packet = new Lazy<WtLogin>(() => new WtLogin(context));

        return base.Parse(input, context);
    }
}