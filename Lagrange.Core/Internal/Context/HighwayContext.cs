using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks.Sources;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Network;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Internal.Context;

internal class HighwayContext : IClientListener, IDisposable
{
    private const string Tag = nameof(HighwayContext);
    
    private readonly BotContext _context;

    private readonly HttpClient _client;
    
    private readonly ObjectPool<CallbackClientListener> _sockets;

    private readonly Dictionary<int, HighwayValueTaskSource> _tasks = new();
    
    private readonly ulong _chunkSize;

    private readonly int _concurrent;
    
    private int _sequence;

    private (byte[], DateTime)? _ticket;

    private string? _url;
    
    public HighwayContext(BotContext context)
    {
        _context = context;

        _client = new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });
        _client.DefaultRequestHeaders.Add("Accept-Encoding", "identity");
        _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2)");
        _sockets = new ObjectPool<CallbackClientListener>(() => new CallbackClientListener(this), t => t.Disconnect());
        
        _sequence = 0;
        _chunkSize = context.Config.HighwayChunkSize;
        _concurrent = (int)context.Config.HighwayConcurrent;
    }

    public uint HeaderSize => 1 + 4 + 4;
    
    public uint GetPacketLength(ReadOnlySpan<byte> header) => 1 + 1 + 4 + 4 + BinaryPrimitives.ReadUInt32BigEndian(header[1..]) + BinaryPrimitives.ReadUInt32BigEndian(header[5..]);

    public void OnRecvPacket(ReadOnlySpan<byte> packet)
    {
        var reader = new BinaryPacket(packet);

        if (reader.Read<byte>() == 0x28)
        {
            int headLen = reader.Read<int>();
            int bodyLen = reader.Read<int>();
            var head = reader.CreateSpan(headLen);
            var body = GC.AllocateUninitializedArray<byte>(bodyLen);
            reader.ReadBytes(body.AsSpan());

            if (reader.Read<byte>() == 0x29)
            {
                var obj = ProtoHelper.Deserialize<RespDataHighwayHead>(head);
                _tasks[(int)obj.MsgBaseHead.Seq].SetResult((obj, body));
            }
        }
    }

    public void OnDisconnect() { }

    public void OnSocketError(Exception e, ReadOnlyMemory<byte> data = default)
    {
        _context.LogError(Tag, "Highway Socket error: {0}", e.Message);
        if (e.StackTrace is { } stack) _context.LogDebug(Tag, stack);
    }
    
    public async Task<bool> UploadFile(Stream stream, int commandId, ReadOnlyMemory<byte> extendInfo)
    {
        if (_ticket == null || _url == null || DateTime.Now - _ticket.Value.Item2 > TimeSpan.FromDays(0.5))
        {
            var resp = await _context.EventContext.SendEvent<HighwaySessionEventResp>(new HighwaySessionEventReq());
            _ticket = (resp.SigSession, DateTime.Now);
            _url = resp.HighwayUrls[1][0];
        }
        
        var tasks = new List<Task<bool>>();
        bool result = true;

        ulong fileSize = (ulong)stream.Length;
        ulong offset = 0;
        int sequence = GetNewSequence();
        var fileMd5 = stream.Md5();
        while (offset < fileSize)
        {
            var buffer = ArrayPool<byte>.Shared.Rent((int)_chunkSize);
            ulong payload = (ulong)await stream.ReadAsync(buffer.AsMemory(0, (int)_chunkSize));

            ulong currentBlockOffset = offset;
            var task = Task.Run(async () => // closure
            {
                var head = new DataHighwayHead
                {
                    Version = 1,
                    Uin = _context.Keystore.Uin.ToString(),
                    Command = "PicUp.DataUp",
                    Seq = (uint)sequence,
                    AppId = (uint)_context.AppInfo.SubAppId,
                    DataFlag = 16,
                    CommandId = (uint)commandId,
                };
                var segHead = new SegHead
                {
                    Filesize = fileSize,
                    DataOffset = currentBlockOffset,
                    DataLength = (uint)payload,
                    ServiceTicket = _ticket.Value.Item1,
                    Md5 = MD5.HashData(buffer.AsSpan(0, (int)payload)),
                    FileMd5 = fileMd5,
                };
                var loginHead = new LoginSigHead
                {
                    Uint32LoginSigType = 8,
                    BytesLoginSig = _context.Keystore.WLoginSigs.A2,
                    AppId = (uint)_context.AppInfo.AppId
                };
                var highwayHead = new ReqDataHighwayHead
                {
                    MsgBaseHead = head,
                    MsgSegHead = segHead,
                    BytesReqExtendInfo = extendInfo,
                    Timestamp = 0,
                    MsgLoginSigHead = loginHead
                };
                var headProto = ProtoHelper.Serialize(highwayHead);

                if (!_context.Config.UseHttpHighway)
                {
                    bool end = currentBlockOffset + payload >= fileSize;
                    var upload = ArrayPool<byte>.Shared.Rent(1 + 1 + 4 + 4 + headProto.Length + (int)payload);

                    upload[0] = 0x28;
                    BinaryPrimitives.WriteUInt32BigEndian(upload.AsSpan(1), (uint)headProto.Length);
                    BinaryPrimitives.WriteUInt32BigEndian(upload.AsSpan(5), (uint)payload);
                    headProto.Span.CopyTo(upload.AsSpan(9));
                    buffer.AsSpan(0, (int)payload).CopyTo(upload.AsSpan(9 + headProto.Length));
                    upload[^1] = 0x29;

                    var content = new ByteArrayContent(upload);
                    var request = new HttpRequestMessage(HttpMethod.Post, $"http://{_url}")
                    {
                        Content = content, Headers = { { "Connection", end ? "close" : "keep-alive" } }
                    };

                    try
                    {
                        var response = await _client.SendAsync(request);
                        var reader = new BinaryPacket((await response.Content.ReadAsByteArrayAsync()).AsSpan());

                        if (reader.Read<byte>() == 0x28)
                        {
                            int headLen = reader.Read<int>();
                            int bodyLen = reader.Read<int>();
                            var respHead = reader.CreateSpan(headLen);
                            var body = GC.AllocateUninitializedArray<byte>(bodyLen);
                            reader.ReadBytes(body.AsSpan());

                            if (reader.Read<byte>() == 0x29)
                            {
                                var obj = ProtoHelper.Deserialize<RespDataHighwayHead>(respHead);
                                _context.LogDebug(Tag, "Highway Block Result: {0} | {1} | {2}", obj.ErrorCode, obj.MsgSegHead?.RetCode, Convert.ToHexString(body));
                                return obj.ErrorCode == 0;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _context.LogError(Tag, "Highway HTTP error: {0}", e.Message);
                        if (e.StackTrace is { } stack) _context.LogDebug(Tag, stack);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(upload);
                        request.Dispose();
                        content.Dispose();
                    }

                    return false;
                }

                var client = _sockets.Get();
                if (client.Connected) client.Disconnect();
                    
                try
                {
                    string url = _url.Split(":")[0];
                    ushort port = ushort.Parse(_url.Split(":")[1]);
                    if (!await client.Connect(url, port)) return false;
                        
                    var tcs = new HighwayValueTaskSource();
                    _tasks[sequence] = tcs;

                    var length = new byte[8];
                    BinaryPrimitives.WriteUInt32BigEndian(length.AsSpan(0), (uint)headProto.Length);
                    BinaryPrimitives.WriteUInt32BigEndian(length.AsSpan(4), (uint)payload);

                    await client.Send(new ReadOnlyMemory<byte>([0x28]), SocketFlags.Partial);
                    await client.Send(length, SocketFlags.Partial);
                    await client.Send(headProto, SocketFlags.Partial);
                    await client.Send(buffer.AsMemory(0, (int)payload), SocketFlags.Partial);
                    await client.Send(new ReadOnlyMemory<byte>([0x29]));

                    var (respHead, resp) = await new ValueTask<(RespDataHighwayHead, byte[])>(tcs, 0);

                    _context.LogDebug(Tag, "Highway Block Result: {0} | {1} | {2}", respHead.ErrorCode, respHead.MsgSegHead?.RetCode, Convert.ToHexString(resp));
                    return respHead.ErrorCode == 0;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);

                    client.Disconnect();
                    _sockets.Return(client);
                }
            });


            tasks.Add(task);
            if (tasks.Count == (commandId == 95 ? 1 : _concurrent))
            {
                var successBlocks = await Task.WhenAll(tasks);
                foreach (bool t in successBlocks) result &= t;
                tasks.Clear();
            }
            
            offset += payload;
        }
        
        if (tasks.Count != 0)
        {
            var finalBlocks = await Task.WhenAll(tasks);
            foreach (bool t in finalBlocks) result &= t;
            tasks.Clear();
        }
        
        return result;
    }

    public void Dispose()
    {
        _sockets.Dispose();
    }

    private int GetNewSequence()
    {
        Interlocked.CompareExchange(ref _sequence, 0, 100000);
        return Interlocked.Increment(ref _sequence);
    }
    
    private sealed class HighwayValueTaskSource : IValueTaskSource<(RespDataHighwayHead, byte[])> 
    {
        private ManualResetValueTaskSourceCore<(RespDataHighwayHead, byte[])> _core;
    
        public (RespDataHighwayHead, byte[]) GetResult(short token) => _core.GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags) => _core.OnCompleted(continuation, state, token, flags);

        public void SetResult((RespDataHighwayHead, byte[]) result) => _core.SetResult(result);

        public void SetException(Exception exception) => _core.SetException(exception);
    }
    
    private sealed class ObjectPool<T>(Func<T> objectGenerator, Action<T> onDispose) : IDisposable
    {
        private readonly ConcurrentBag<T> _objects = [];

        public T Get() => _objects.TryTake(out var item) ? item : objectGenerator();

        public void Return(T item) => _objects.Add(item);

        public void Dispose()
        {
            foreach (var obj in _objects) onDispose(obj);
        }
    }
}