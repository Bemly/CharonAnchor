using Lagrange.Core.Common;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Compression;

namespace Lagrange.Core.Internal.Packets.Struct;

internal class SsoPacker(BotContext context) : StructBase(context)
{
    public BinaryPacket BuildProtocol12(SsoPacket sso, SsoSecureInfo? secInfo) // TODO: sso_reserve_fields
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
        head.Write([0], Prefix.Int32 | Prefix.WithPrefix); // sso_reserve_fields
        
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
        head.Write([0], Prefix.Int32 | Prefix.WithPrefix); // TODO:sso_reserve_fields
        
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
        int sequence = headReader.ReadInt32();
        int retCode = headReader.ReadInt32();
        string extra = headReader.ReadString(Prefix.Int32 | Prefix.WithPrefix);
        string command = headReader.ReadString(Prefix.Int32 | Prefix.WithPrefix);
        var msgCookie = headReader.ReadBytes(Prefix.Int32 | Prefix.WithPrefix);
        int dataFlag = headReader.ReadInt32();
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
}