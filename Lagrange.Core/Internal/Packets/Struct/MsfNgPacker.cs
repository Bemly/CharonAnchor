using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Services;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Internal.Packets.Struct;

internal class MsfNgPacker(BotContext context) : StructBase(context)
{
    private static readonly byte[] ZeroKey = new byte[16];

    public byte[]? SessionKey { get; set; }

    public uint Sequence { get; set; }

    public ReadOnlyMemory<byte> BuildProtocol12(BotSsoPacket sso, ServiceAttribute options) => BuildFrame(sso, options, 12);

    public ReadOnlyMemory<byte> BuildProtocol13(BotSsoPacket sso, ServiceAttribute options) => BuildFrame(sso, options, 13);

    public static byte[] BuildPing(uint uin, uint index)
    {
        byte[] ping =
        [
            0x00, 0x00, 0x00, 0x15,
            0x01, 0x33, 0x52, 0x39,
            0x00, 0x00, 0x00, 0x00,
            0x04, 0x4D, 0x53, 0x46, 0x05,
            0x00, 0x00, 0x00, 0x00
        ];
        BinaryPrimitives.WriteUInt32BigEndian(ping.AsSpan(8), uin);
        BinaryPrimitives.WriteUInt32BigEndian(ping.AsSpan(17), index);
        return ping;
    }

    public ReadOnlyMemory<byte> BuildUnauthenticatedFrame(string command, ReadOnlySpan<byte> body, string uin, int protocol = 13)
    {
        var head = new BinaryPacket(stackalloc byte[0x200]);
        head.EnterLengthBarrier<int>();
        head.Write(command, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(BuildTrace(), Prefix.Int32 | Prefix.WithPrefix);
        head.ExitLengthBarrier<int>(true);
        head.Write(8);
        head.Write(4);

        var headSpan = head.CreateReadOnlySpan();
        var writer = new BinaryPacket(headSpan.Length + body.Length + 0x80);
        writer.EnterLengthBarrier<int>();
        writer.Write(protocol);
        writer.Write((byte)MsfNgEncrypt.Plain);
        if (uin.Length > 0)
        {
            writer.Write(uin, Prefix.Int32 | Prefix.WithPrefix);
        }
        else
        {
            writer.Write(4);
        }
        writer.Write((byte)0);
        writer.Write(string.Empty, Prefix.Int32 | Prefix.WithPrefix);
        writer.Write(headSpan);
        writer.Write(body);
        writer.ExitLengthBarrier<int>(true);
        head.Dispose();

        return writer.ToArray();
    }

    private ReadOnlyMemory<byte> BuildFrame(BotSsoPacket sso, ServiceAttribute options, int protocol)
    {
        byte flag = options.EncryptType switch
        {
            EncryptType.NoEncrypt => (byte)MsfNgEncrypt.Plain,
            EncryptType.EncryptD2Key => (byte)MsfNgEncrypt.Session,
            EncryptType.EncryptEmpty => (byte)MsfNgEncrypt.ZeroKey,
            _ => throw new ArgumentOutOfRangeException(nameof(options.EncryptType), options.EncryptType, null)
        };

        var head = new BinaryPacket(stackalloc byte[0x200]);
        head.EnterLengthBarrier<int>();
        head.Write(sso.Command, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(BuildTrace(), Prefix.Int32 | Prefix.WithPrefix);
        head.ExitLengthBarrier<int>(true);
        head.Write(8);
        head.Write(4);

        // EncodeBusiBuff: busi wire format = [u32 len+4][data], omitted entirely when empty;
        // EncodeFinal encrypts head+busi together as one region when enc != 0
        var headSpan = head.CreateReadOnlySpan();
        bool hasBusi = !sso.Data.IsEmpty;
        int plainLength = headSpan.Length + (hasBusi ? 4 + sso.Data.Length : 0);
        byte[] plain = new byte[plainLength];
        headSpan.CopyTo(plain);
        if (hasBusi)
        {
            BinaryPrimitives.WriteInt32BigEndian(plain.AsSpan(headSpan.Length), sso.Data.Length + 4);
            sso.Data.Span.CopyTo(plain.AsSpan(headSpan.Length + 4));
        }

        ReadOnlySpan<byte> cipher = flag switch
        {
            (byte)MsfNgEncrypt.Plain => plain,
            (byte)MsfNgEncrypt.Session => SessionKey is not null
                ? TeaProvider.Encrypt(plain, SessionKey)
                : throw new InvalidOperationException("MSF-NG session key has not been established"),
            (byte)MsfNgEncrypt.ZeroKey => TeaProvider.Encrypt(plain, ZeroKey),
            _ => throw new UnreachableException()
        };

        var writer = new BinaryPacket(cipher.Length + 0x40);
        writer.EnterLengthBarrier<int>();
        writer.Write(protocol);
        writer.Write(flag);
        writer.Write((uint)sso.Sequence);
        writer.Write((byte)0);
        writer.Write(string.Empty, Prefix.Int32 | Prefix.WithPrefix);
        writer.Write(cipher);
        writer.ExitLengthBarrier<int>(true);
        head.Dispose();

        return writer.ToArray();
    }

    public MsfNgPacket? Parse(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < 19) return null;

        uint version = BinaryPrimitives.ReadUInt32BigEndian(buffer[4..]);
        if (version is not (12 or 13 or 20 or 21)) return null;

        byte enc = buffer[8];
        uint basicSequence = BinaryPrimitives.ReadUInt32BigEndian(buffer[9..]);
        int cmdLength = BinaryPrimitives.ReadInt32BigEndian(buffer[14..]);
        if (cmdLength < 4 || 14 + cmdLength > buffer.Length) return null; // channel level frame e.g. pong

        ReadOnlySpan<byte> region = buffer[(14 + cmdLength)..];
        if (enc != 0)
        {
            var key = enc switch
            {
                (byte)MsfNgEncrypt.Session => SessionKey,
                (byte)MsfNgEncrypt.ZeroKey => ZeroKey,
                _ => null
            };
            if (key is null) return null;
            region = TeaProvider.Decrypt(region, key);
        }

        if (region.Length < 12) return null;

        uint headLength = BinaryPrimitives.ReadUInt32BigEndian(region);
        uint headSequence = BinaryPrimitives.ReadUInt32BigEndian(region[4..]);
        int retCode = (int)BinaryPrimitives.ReadUInt32BigEndian(region[8..]);

        int position = 12;
        string extra = ReadLengthPrefixedString(region, ref position) ?? string.Empty;
        string command = ReadLengthPrefixedString(region, ref position) ?? string.Empty;

        var payload = region[(int)Math.Min(headLength, (uint)region.Length)..];

        return new MsfNgPacket(command, payload.ToArray(), (int)headSequence, (int)basicSequence, retCode, extra);
    }

    private byte[] BuildTrace()
    {
        string hash = Convert.ToHexString(MD5.HashData(Keystore.Guid)).ToLowerInvariant();
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

        var trace = new byte[69];
        trace[0] = (byte)'b';
        trace[1] = (byte)' ';
        Encoding.ASCII.GetBytes(hash, trace.AsSpan(2));
        int offset = 34;
        trace[offset++] = 0xBA;
        trace[offset++] = 0x01;
        trace[offset++] = 29;
        trace[offset++] = 0x0A;
        trace[offset++] = 15;
        Encoding.ASCII.GetBytes("client_conn_seq", trace.AsSpan(offset));
        offset += 15;
        trace[offset++] = 0x12;
        trace[offset++] = 10;
        Encoding.ASCII.GetBytes(timestamp, trace.AsSpan(offset));
        trace[^3] = 0xD0;
        trace[^2] = 0x01;
        trace[^1] = 0x65;
        return trace;
    }

    private static string? ReadLengthPrefixedString(ReadOnlySpan<byte> region, ref int position)
    {
        if (position + 4 > region.Length) return null;

        int length = BinaryPrimitives.ReadInt32BigEndian(region[position..]);
        position += 4;
        if (length < 4 || position + length - 4 > region.Length) return null;

        string value = Encoding.UTF8.GetString(region[position..(position + length - 4)]);
        position += length - 4;
        return value;
    }
}

internal enum MsfNgEncrypt : byte
{
    Plain = 0,
    Session = 1,
    ZeroKey = 2,
}

internal class MsfNgPacket(
    string command,
    ReadOnlyMemory<byte> payload,
    int headSequence,
    int basicSequence,
    int retCode,
    string extra)
{
    public string Command { get; } = command;

    public ReadOnlyMemory<byte> Payload { get; } = payload;

    public int HeadSequence { get; } = headSequence;

    public int BasicSequence { get; } = basicSequence;

    public int RetCode { get; } = retCode;

    public string Extra { get; } = extra;
}
