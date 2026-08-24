using System.Buffers.Binary;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Services;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Runner;

internal static class MsfNgProbe
{
    private const string Host = "msfwifi.3g.qq.com";
    private const int Port = 8080;
    private const string EstablishCommand = "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey";

    public static async Task<int> RunAsync(string[] args)
    {
        long uin = args.Length > 0 && long.TryParse(args[0], out var parsed) ? parsed : 0;
        string dumpDir = Path.Combine(Directory.GetCurrentDirectory(), "msfng-probe");
        Directory.CreateDirectory(dumpDir);

        using var client = new TcpClient();
        await client.ConnectAsync(Host, Port);
        Console.WriteLine($"[probe] connected {Host}:{Port}");
        await using var stream = client.GetStream();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        byte[] ping = MsfNgPacker.BuildPing((uint)uin, 1);
        await stream.WriteAsync(ping, cts.Token);
        byte[]? pong = await ReadFrameAsync(stream, cts.Token);
        Console.WriteLine($"[probe] ping -> {(pong is null ? "NO PONG" : $"pong {pong.Length}B")}");

        var context = new BotContext(new BotConfig(), new BotKeystore
        {
            Uin = uin,
            Guid = RandomNumberGenerator.GetBytes(16)
        }, BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
        var packer = new MsfNgPacker(context);

        const string keyExchangeCommand = "trpc.login.ecdh.EcdhService.SsoKeyExchange";
        byte[] headEmptyReserve = BuildHeadV13(packer, keyExchangeCommand, "", new byte[] { 0x08, 0x04 });
        byte[] headUinStr2 = BuildHeadV13(packer, keyExchangeCommand, uin.ToString(), new byte[] { 0x08, 0x04 });
        byte[] headUidField = BuildHeadV13(packer, keyExchangeCommand, "", new byte[] { 0x08, 0x04 });

        var variants = new (string Name, Func<uint, ReadOnlyMemory<byte>> Build)[]
        {
            ("head-reserve-f12", s => AssembleSeqFrame(13, s, headEmptyReserve, MsfNgKeyExchange.BuildRequestVariant(out _, keyExchangeCommand, ReadOnlyMemory<byte>.Empty, TsEncoding.VarInt))),
            ("head-str2-uin", s => AssembleSeqFrame(13, s, headUinStr2, MsfNgKeyExchange.BuildRequestVariant(out _, keyExchangeCommand, ReadOnlyMemory<byte>.Empty, TsEncoding.VarInt))),
        };

        uint seq = 0x623800;
        foreach (var variant in variants)
        {
            seq++;
            ReadOnlyMemory<byte> frame = variant.Build(seq);

            Console.WriteLine($"[probe] === variant={variant.Name} seq=0x{seq:X} frame={frame.Length}B");
            await stream.WriteAsync(frame, cts.Token);

            byte[]? response = await ReadFrameWithTimeoutAsync(stream, TimeSpan.FromSeconds(8));
            if (response is null)
            {
                Console.WriteLine("[probe] no response");
                continue;
            }

            string file = Path.Combine(dumpDir, $"rsp-{variant.Name}-{seq:X}.bin");
            await File.WriteAllBytesAsync(file, response, cts.Token);
            Console.WriteLine($"[probe] <- ({response.Length}B): {Convert.ToHexString(response)}");
            string decoded = DecodeServerResponse(response);
            Console.WriteLine($"[probe] decoded: {decoded}");
        }

        return 0;
    }

    private static byte[] BuildHead(MsfNgPacker packer, string command)
    {
        var method = typeof(MsfNgPacker).GetMethod("BuildTrace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var trace = (byte[])method!.Invoke(packer, null)!;

        using var head = new BinaryPacket(stackalloc byte[0x200]);
        head.EnterLengthBarrier<int>();
        head.Write(command, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(ReadOnlySpan<byte>.Empty, Prefix.Int32 | Prefix.WithPrefix);
        head.Write(trace, Prefix.Int32 | Prefix.WithPrefix);
        head.ExitLengthBarrier<int>(true);
        head.Write(8);
        head.Write(4);

        return head.CreateReadOnlySpan().ToArray();
    }

    private static ReadOnlyMemory<byte> AssembleSeqFrame(int protocol, uint seq, byte[] headAndBody, ReadOnlyMemory<byte> busi)
    {
        using var writer = new BinaryPacket(headAndBody.Length + busi.Length + 0x40);
        writer.EnterLengthBarrier<int>();
        writer.Write(protocol);
        writer.Write((byte)MsfNgEncrypt.Plain);
        writer.Write(seq);
        writer.Write((byte)0);
        writer.Write(string.Empty, Prefix.Int32 | Prefix.WithPrefix);
        writer.Write(headAndBody);
        writer.Write(busi.Span);
        writer.ExitLengthBarrier<int>(true);
        return writer.ToArray();
    }

    private static byte[] BuildHeadV13(MsfNgPacker packer, string command, string secondString, byte[] reservePb)
    {
        var method = typeof(MsfNgPacker).GetMethod("BuildTrace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        _ = method!.Invoke(packer, null);

        using var writer = new BinaryPacket(stackalloc byte[0x200]);
        writer.EnterLengthBarrier<int>();
        writer.Write(command, Prefix.Int32 | Prefix.WithPrefix);
        writer.Write(secondString, Prefix.Int32 | Prefix.WithPrefix);

        // reserve fields: outer barrier incl self wrapping inner barrier incl self + protobuf
        int reserveLen = 4 + 4 + reservePb.Length;
        writer.Write(reserveLen);
        writer.Write(4 + reservePb.Length);
        writer.Write(reservePb);
        writer.ExitLengthBarrier<int>(true);
        return writer.CreateReadOnlySpan().ToArray();
    }

    private static string DecodeServerResponse(byte[] response)
    {
        if (response.Length < 15) return "too short";

        byte enc = response[8];
        if (enc == 0) return DescribePlain(response.AsSpan()[23..]);

        // brute force zero-key TEA at every aligned offset
        for (int offset = 9; offset < Math.Min(response.Length - 15, 64); offset++)
        {
            var cipher = response.AsSpan()[offset..];
            if (cipher.Length % 8 != 0) continue;
            try
            {
                var dest = new byte[cipher.Length];
                TeaProvider.Decrypt(cipher, dest, ZeroTeaKey);
                for (int i = dest.Length - 7; i < dest.Length; i++)
                {
                    if (dest[i] != 0) throw new InvalidDataException();
                }
                var plain = dest[((dest[0] & 7) + 3)..^7].AsSpan();
                string desc = DescribePlain(plain);
                if (desc.Contains("failed") || desc.Contains('"') || plain.Length > 10)
                    return $"tea@{offset}: {desc}";
            }
            catch
            {
                // ignore invalid padding candidates
            }
        }
        return "undecodable";
    }

    private static readonly byte[] ZeroTeaKey = new byte[16];

    private static string DescribePlain(ReadOnlySpan<byte> plain)
    {
        var sb = new StringBuilder();
        int position = 0;
        while (position + 4 <= plain.Length && sb.Length < 400)
        {
            uint value = BinaryPrimitives.ReadUInt32BigEndian(plain[position..]);
            if (value >= 4 && value <= plain.Length - position - 4 && IsPrintableAscii(plain.Slice(position + 4, (int)value - 4)))
            {
                var text = Encoding.ASCII.GetString(plain.Slice(position + 4, (int)value - 4));
                sb.Append($"str({value})=\"{text}\" ");
                position += 4 + (int)value - 4;
            }
            else
            {
                sb.Append($"0x{value:X} ");
                position += 4;
            }
        }
        return sb.ToString();
    }

    private static bool IsPrintableAscii(ReadOnlySpan<byte> data)
    {
        foreach (byte b in data)
        {
            if (b is < 0x20 or > 0x7E) return false;
        }
        return data.Length > 0;
    }

    private static async Task<byte[]?> ReadFrameWithTimeoutAsync(NetworkStream stream, TimeSpan timeout)
    {
        try
        {
            using var cts = new CancellationTokenSource(timeout);
            return await ReadFrameAsync(stream, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static async Task<byte[]?> ReadFrameAsync(NetworkStream stream, CancellationToken token)
    {
        try
        {
            var header = new byte[4];
            if (!await TryReadExactAsync(stream, header, token)) return null;

            int length = BinaryPrimitives.ReadInt32BigEndian(header);
            if (length is < 4 or > 4 * 1024 * 1024) return null;

            var payload = new byte[length];
            header.CopyTo(payload, 0);
            if (!await TryReadExactAsync(stream, payload.AsMemory()[4..], token)) return null;

            return payload;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    private static async Task<bool> TryReadExactAsync(NetworkStream stream, Memory<byte> buffer, CancellationToken token)
    {
        while (buffer.Length > 0)
        {
            int read = await stream.ReadAsync(buffer, token);
            if (read <= 0) return false;
            buffer = buffer[read..];
        }
        return true;
    }
}
