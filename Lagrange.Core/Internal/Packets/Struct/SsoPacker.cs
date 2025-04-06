using Lagrange.Core.Common;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Compression;

namespace Lagrange.Core.Internal.Packets.Struct;

internal class SsoPacker(BotContext context) : StructBase(context)
{
    private const string Hex = "0123456789abcdef";
    
    public BinaryPacket BuildProtocol12(SsoPacket sso, SsoSecureInfo? secInfo)
    {
        var head = new BinaryPacket(stackalloc byte[0x200]);
        
        head.Write(sso.Sequence); // sequence
        head.Write(AppInfo.SubAppId); // subAppId
        head.Write(2052); // unknown
        head.Write([0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
        head.Write(Keystore.WLoginSigs.A2, Prefix.Int32 | Prefix.WithPrefix); // tgt
        head.Write(sso.Command, Prefix.Int32 | Prefix.WithPrefix); // command
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix); // message_cookies
        head.Write(Keystore.Guid, Prefix.Int32 | Prefix.WithPrefix); // guid
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(AppInfo.CurrentVersion, Prefix.Int16 | Prefix.WithPrefix);
        WriteSsoReservedField(ref head, secInfo);
        
        var headSpan = head.CreateReadOnlySpan();
        var result = new BinaryPacket(headSpan.Length + sso.Data.Length + 2 * 4); // 2 * 4 for the length of the payload
        
        result.Write(headSpan, Prefix.Int32 | Prefix.WithPrefix);
        result.Write(sso.Data.Span, Prefix.Int32 | Prefix.WithPrefix); // payload
        
        return result;
    }
    
    public BinaryPacket BuildProtocol13(SsoPacket sso)
    {
        var head = new BinaryPacket(stackalloc byte[0x200]);
        
        head.Write(sso.Command, Prefix.Int32 | Prefix.WithPrefix); // command
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix); // message_cookies
        WriteSsoReservedField(ref head, null);
        
        var headSpan = head.CreateReadOnlySpan();
        var result = new BinaryPacket(headSpan.Length + sso.Data.Length + 2 * 4); // 2 * 4 for the length of the payload
        
        result.Write(headSpan, Prefix.Int32 | Prefix.WithPrefix);
        result.Write(sso.Data.Span, Prefix.Int32 | Prefix.WithPrefix); // payload
        
        return result;
    }

    public SsoPacket Parse(ReadOnlySpan<byte> data)
    {
        var parent = new BinaryPacket(data);
        var head = parent.ReadBytes(Prefix.Int32 | Prefix.WithPrefix);
        var body = parent.ReadBytes(Prefix.Int32 | Prefix.WithPrefix);
        
        var headReader = new BinaryPacket(head);
        int sequence = headReader.Read<int>();
        int retCode = headReader.Read<int>();
        string extra = headReader.ReadString(Prefix.Int32 | Prefix.WithPrefix);
        string command = headReader.ReadString(Prefix.Int32 | Prefix.WithPrefix);
        var msgCookie = headReader.ReadBytes(Prefix.Int32 | Prefix.WithPrefix);
        int dataFlag = headReader.Read<int>();
        var reserveField = headReader.ReadBytes(Prefix.Int32 | Prefix.WithPrefix);
        
        ReadOnlyMemory<byte> payload = dataFlag switch
        {
            0 or 4 => body.ToArray(), // allocation
            1 => ZCompression.ZDecompress(body, false),
            _ => throw new ArgumentOutOfRangeException(nameof(dataFlag))
        };

        return retCode == 0
            ? new SsoPacket(command, sequence, retCode, extra)
            : new SsoPacket(command, payload, sequence);
    }

    private void WriteSsoReservedField(ref BinaryPacket writer, SsoSecureInfo? secInfo)
    {
        Span<char> trace = stackalloc char[55];

        trace[0] = '0';
        trace[1] = '1';
        trace[2] = '-';
        for (int i = 3; i < 35; i++) trace[i] = Hex[Random.Shared.Next(0, Hex.Length)];
        trace[35] = '-';
        for (int i = 36; i < 52; i++) trace[i] = Hex[Random.Shared.Next(0, Hex.Length)];
        trace[52] = '-';
        trace[53] = '0';
        trace[54] = '1';
        
        var reserved = new SsoReserveFields
        {
            TraceParent = new string(trace),
            Uid = Keystore.Uid,
            SecInfo = secInfo
        };
     
        writer.EnterLengthBarrier<int>();
        ProtoHelper.Serialize(ref writer, reserved);
        writer.ExitLengthBarrier<int>(true);
    }
}