using System.Buffers.Binary;
using System.Diagnostics;
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
        string signDir = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();
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

        byte[] head = BuildHeadV13(packer, keyExchangeCommand, "", new byte[] { 0x08, 0x04 });

        uint seq = 0x623800;

        async Task SendAndLog(string name, ReadOnlyMemory<byte> frame, int observeSeconds = 8)
        {
            Console.WriteLine($"[probe] === {name} frame={frame.Length}B");
            await stream.WriteAsync(frame, cts.Token);

            var watch = Stopwatch.StartNew();
            while (watch.Elapsed.TotalSeconds < observeSeconds)
            {
                byte[]? response = await ReadFrameWithTimeoutAsync(stream, TimeSpan.FromSeconds(Math.Min(5, observeSeconds - watch.Elapsed.TotalSeconds)));
                if (response is null)
                {
                    if (!client.Connected)
                    {
                        Console.WriteLine("[probe] connection closed by server");
                        return;
                    }
                    continue;
                }
                string file = Path.Combine(dumpDir, $"rsp-{name}-{DateTime.Now:HHmmssfff}.bin");
                await File.WriteAllBytesAsync(file, response, cts.Token);
                Console.WriteLine($"[probe] <- ({response.Length}B) t+{watch.Elapsed.TotalSeconds:F1}s: {Convert.ToHexString(response)}");
                Console.WriteLine($"[probe] decoded: {DecodeServerResponse(response)}");
            }
            Console.WriteLine($"[probe] observation ended, connected={client.Connected}");
        }

        // experiment: REAL signature from wrapper.node in reserve.f24
        var signProvider = new CharonAnchor.CharonSignProvider(signDir, "3.2.32");
        using (signProvider)
        {
            var kxRequest = MsfNgKeyExchange.BuildRequestVariant(out _, keyExchangeCommand, ReadOnlyMemory<byte>.Empty, TsEncoding.VarInt);
            var secInfo = await signProvider.GetSecSign(uin, keyExchangeCommand, (int)(++seq), kxRequest);
            if (secInfo is null)
            {
                Console.WriteLine("[probe] GetSecSign failed");
                return 3;
            }
            Console.WriteLine($"[probe] secSig={secInfo.SecSign.Length}B token={secInfo.SecToken.Length}B extra={secInfo.SecExtra.Length}B");

            byte[] headReal = BuildHeadV13WithSigs(packer, keyExchangeCommand, secInfo.SecSign, secInfo.SecToken, secInfo.SecExtra);
            await SendAndLog("kx-f24-real", AssembleSeqFrame(13, ++seq, headReal, BuildKxBusi(keyExchangeCommand, 0)), observeSeconds: 12);
        }

        Console.WriteLine("[probe] done");
        return 0;
    }

    private static byte[] BuildHeadV13WithSigs(MsfNgPacker packer, string command, byte[] secSig, byte[] deviceToken, byte[] extra)
    {
        var sigs = new ProtoWriter();
        sigs.WriteBytes(1, secSig);
        sigs.WriteBytes(2, deviceToken);
        sigs.WriteBytes(3, extra);

        var reserve = new ProtoWriter();
        reserve.WriteBytes(13, Encoding.UTF8.GetBytes("01-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant() + "-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant() + "-01"));
        reserve.WriteVarInt(21, 32);
        reserve.WriteBytes(24, sigs.ToArray());

        using var writer = new BinaryPacket(stackalloc byte[0x200]);
        writer.EnterLengthBarrier<int>();
        writer.Write(command, Prefix.Int32 | Prefix.WithPrefix);
        writer.Write("", Prefix.Int32 | Prefix.WithPrefix);

        byte[] reserveBytes = reserve.ToArray();
        writer.Write(reserveBytes.Length + 4);
        writer.Write(4 + reserveBytes.Length);
        writer.Write(reserveBytes);
        writer.ExitLengthBarrier<int>(true);
        return writer.CreateReadOnlySpan().ToArray();
    }

    private static byte[] BuildKxBusi(string command, int sceneId)
    {
        var request = MsfNgKeyExchange.BuildRequestVariant(out _, command, ReadOnlyMemory<byte>.Empty, TsEncoding.VarInt);
        var inner = new ProtoWriter();
        inner.WriteVarInt(1, (ulong)sceneId);
        inner.WriteBytes(2, Encoding.UTF8.GetBytes(command));
        var body = new ProtoWriter();
        body.WriteBytes(2, request);
        body.WriteBytes(3, inner.ToArray());
        return body.ToArray();
    }

    private static byte[] BuildHeadReserve(MsfNgPacker packer, string command)
    {
        string hex = "0123456789abcdef";
        var traceChars = new char[55];
        traceChars[0] = '0'; traceChars[1] = '1'; traceChars[2] = '-';
        for (int i = 3; i < 35; i++) traceChars[i] = hex[RandomNumberGenerator.GetInt32(16)];
        traceChars[35] = '-';
        for (int i = 36; i < 52; i++) traceChars[i] = hex[RandomNumberGenerator.GetInt32(16)];
        traceChars[52] = '-'; traceChars[53] = '0'; traceChars[54] = '1';
        string traceParent = new(traceChars);

        var reserve = new ProtoWriter();
        reserve.WriteBytes(12, new byte[] { 0x01 });            // f12: placeholder uid marker
        reserve.WriteBytes(13, Encoding.UTF8.GetBytes(traceParent)); // f13: traceparent
        reserve.WriteVarInt(21, 32);                            // f21: msgType
        reserve.WriteVarInt(26, 100);                           // f26: ntCoreVersion

        using var writer = new BinaryPacket(stackalloc byte[0x200]);
        writer.EnterLengthBarrier<int>();
        writer.Write(command, Prefix.Int32 | Prefix.WithPrefix);
        writer.Write("", Prefix.Int32 | Prefix.WithPrefix);

        int reserveLen = 4 + 4 + reserve.ToArray().Length;
        writer.Write(reserveLen);
        writer.Write(4 + reserve.ToArray().Length);
        writer.Write(reserve.ToArray());
        writer.ExitLengthBarrier<int>(true);
        return writer.CreateReadOnlySpan().ToArray();
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
        if (busi.Length > 0)
        {
            writer.Write(busi.Length + 4);
            writer.Write(busi.Span);
        }
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

    private static ReadOnlyMemory<byte> AssembleWrapped(int protocol, uint seq, byte[] head, string command, int sceneId)
    {
        var request = MsfNgKeyExchange.BuildRequestVariant(out _, command, ReadOnlyMemory<byte>.Empty, TsEncoding.VarInt);

        var inner = new ProtoWriter();
        inner.WriteVarInt(1, (ulong)sceneId);
        inner.WriteBytes(2, Encoding.UTF8.GetBytes(command));

        var body = new ProtoWriter();
        body.WriteBytes(2, request);
        body.WriteBytes(3, inner.ToArray());

        return AssembleSeqFrame(protocol, seq, head, body.ToArray());
    }

    private static ReadOnlyMemory<byte> AssembleTea2Frame(int protocol, uint seq, byte[] headAndBody, string command, int sceneId)
    {
        var request = MsfNgKeyExchange.BuildRequestVariant(out _, command, ReadOnlyMemory<byte>.Empty, TsEncoding.VarInt);
        var inner = new ProtoWriter();
        inner.WriteVarInt(1, (ulong)sceneId);
        inner.WriteBytes(2, Encoding.UTF8.GetBytes(command));
        var body = new ProtoWriter();
        body.WriteBytes(2, request);
        body.WriteBytes(3, inner.ToArray());

        byte[] bodyBytes = body.ToArray();
        var busi = new byte[4 + bodyBytes.Length];
        BinaryPrimitives.WriteInt32BigEndian(busi.AsSpan(), bodyBytes.Length + 4);
        bodyBytes.CopyTo(busi.AsSpan(4));

        var plain = new byte[headAndBody.Length + busi.Length];
        headAndBody.CopyTo(plain, 0);
        busi.CopyTo(plain, headAndBody.Length);

        var cipher = TeaProvider.Encrypt(plain, ZeroTeaKey);

        using var writer = new BinaryPacket(cipher.Length + 0x40);
        writer.EnterLengthBarrier<int>();
        writer.Write(protocol);
        writer.Write((byte)MsfNgEncrypt.ZeroKey);
        writer.Write(seq);
        writer.Write((byte)0);
        writer.Write(string.Empty, Prefix.Int32 | Prefix.WithPrefix);
        writer.Write(cipher);
        writer.ExitLengthBarrier<int>(true);
        return writer.ToArray();
    }

    private static readonly byte[] ZeroTeaKey = new byte[16];

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
