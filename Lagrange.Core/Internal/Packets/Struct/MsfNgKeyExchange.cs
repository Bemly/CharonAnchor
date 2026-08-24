using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Internal.Packets.Struct;

/// <summary>
/// SsoEstablishShareKey handshake codec reverse engineered from
/// kernel_ecdh_service.cc / ecdh_codec.cc / ecdh_util.cc of QQ 3.2.32 wrapper.node.
/// Request: {1: clientPub, 2: flag=1, 3: AES-GCM(share, inner{1: cmd, 2: body}), 4: unixSec, 5: AES-GCM(hardKey, SHA256(pub++payload++ts))}
/// Response: {1: AES-GCM(share2, secrets), 2: ECDSA-SHA256 signature, 3: serverEphemeralPub}
/// All AES-GCM blobs are IV(12) || CT || TAG(16); share keys are raw P-256 X coordinates.
/// </summary>
internal static class MsfNgKeyExchange
{
    public const string Command = "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey";

    private static readonly byte[] ServerStaticPublicKey =
    [
        0x04, 0x9D, 0x14, 0x23, 0x33, 0x27, 0x35, 0x98, 0x0E, 0xDA, 0xBE, 0x7E, 0x9E, 0xA4, 0x51, 0xB3,
        0x39, 0x5B, 0x6F, 0x35, 0x25, 0x0D, 0xB8, 0xFC, 0x56, 0xF2, 0x58, 0x89, 0xF6, 0x28, 0xCB, 0xAE,
        0x3E, 0x8E, 0x73, 0x07, 0x79, 0x14, 0x07, 0x1E, 0xEE, 0xBC, 0x10, 0x8F, 0x4E, 0x01, 0x70, 0x05,
        0x77, 0x92, 0xBB, 0x17, 0xAA, 0x30, 0x3A, 0xF6, 0x52, 0x31, 0x3D, 0x17, 0xC1, 0xAC, 0x81, 0x5E, 0x79
    ];

    private static readonly byte[] ServerVerifyPublicKey =
    [
        0x04, 0x45, 0x39, 0x77, 0xB0, 0x48, 0xD0, 0xB7, 0x2C, 0x1A, 0x7D, 0x50, 0xC3, 0x6E, 0xBE, 0x88,
        0x1B, 0x69, 0xBB, 0xDD, 0x51, 0xA5, 0xC6, 0x62, 0xD0, 0x8A, 0x1B, 0xAF, 0x12, 0x36, 0xCE, 0x92,
        0xCB, 0xB9, 0x54, 0x60, 0xF5, 0x73, 0xFE, 0x7A, 0x5B, 0x0E, 0xD9, 0xCC, 0xFE, 0xE0, 0x1E, 0xB4,
        0xDF, 0xB6, 0xE6, 0xEC, 0xFA, 0x16, 0xA0, 0x90, 0xE3, 0xED, 0x8F, 0x58, 0x47, 0xA9, 0xDA, 0xF9, 0x84
    ];

    private static readonly byte[] HardAesKey =
    [
        0xE2, 0x73, 0x3B, 0xF4, 0x03, 0x14, 0x99, 0x13, 0xCB, 0xF8, 0x0C, 0x7A, 0x95, 0x16, 0x8B, 0xD4,
        0xCA, 0x69, 0x35, 0xEE, 0x53, 0xCD, 0x39, 0x76, 0x4B, 0xEE, 0xBE, 0x2E, 0x00, 0x7E, 0x3A, 0xEE
    ];

    public static byte[] BuildRequest(out EcdhProvider ephemeral, string command, ReadOnlyMemory<byte> body)
        => BuildRequestVariant(out ephemeral, command, body, TsEncoding.VarInt);

    public static byte[] BuildRequestVariant(out EcdhProvider ephemeral, string command, ReadOnlyMemory<byte> body, TsEncoding tsEncoding, bool flagAsFixed32 = false)
    {
        ephemeral = new EcdhProvider(EllipticCurve.Prime256V1);
        byte[] clientPub = ephemeral.PackPublic(false);
        byte[] share = ephemeral.KeyExchange(ServerStaticPublicKey, false);

        var inner = new ProtoWriter();
        inner.WriteBytes(1, Encoding.UTF8.GetBytes(command));
        inner.WriteBytes(2, body.Span);

        byte[] payload = EncryptGcm(share, inner.ToArray());

        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var digestInput = new ProtoWriter();
        digestInput.WriteRaw(clientPub);
        digestInput.WriteRaw(payload);
        Span<byte> be8 = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(be8, (ulong)timestamp);
        digestInput.WriteRaw(be8);
        byte[] digest = SHA256.HashData(digestInput.ToArray());

        byte[] digestCipher = EncryptGcm(HardAesKey, digest);

        var writer = new ProtoWriter();
        writer.WriteBytes(1, clientPub);
        if (flagAsFixed32) writer.WriteFixed32(2, 1);
        else writer.WriteVarInt(2, 1);
        writer.WriteBytes(3, payload);
        switch (tsEncoding)
        {
            case TsEncoding.VarInt:
                writer.WriteVarInt(4, (ulong)timestamp);
                break;
            case TsEncoding.Fixed64:
                writer.WriteFixed64(4, (ulong)timestamp);
                break;
            default:
                writer.WriteFixed32(4, (uint)timestamp);
                break;
        }
        writer.WriteBytes(5, digestCipher);
        return writer.ToArray();
    }

    public static MsfNgSessionKeys? ParseResponse(ReadOnlySpan<byte> response, EcdhProvider ephemeral)
    {
        if (!ProtoReader.TryReadFields(response, out var fields)) return null;
        if (!fields.TryGetBytes(1, out var encSecrets)) return null;
        if (!fields.TryGetBytes(3, out var serverPub)) return null;

        if (encSecrets.Length < 29) return null;

        byte[] share = ephemeral.KeyExchange(serverPub.ToArray(), false);
        ReadOnlySpan<byte> iv = encSecrets[..12];
        ReadOnlySpan<byte> tag = encSecrets[^16..];
        ReadOnlySpan<byte> cipher = encSecrets[12..^16];

        byte[] plain;
        try
        {
            using var aes = new AesGcm(share, AesGcm.TagByteSizes.MaxSize);
            plain = new byte[cipher.Length];
            aes.Decrypt(iv.ToArray(), cipher.ToArray(), tag.ToArray(), plain);
        }
        catch (AuthenticationTagMismatchException)
        {
            return null;
        }

        if (!ProtoReader.TryReadFields(plain, out var secretFields)) return null;
        if (!secretFields.TryGetBytes(1, out var secret1)) return null;
        secretFields.TryGetBytes(2, out var secret2);
        ulong expiry = secretFields.TryGetVarInt(3, out var value) ? value : 0;

        return new MsfNgSessionKeys(secret1.ToArray(), secret2.ToArray(), (long)expiry);
    }

    public static bool VerifyResponseSignature(ReadOnlySpan<byte> response, ReadOnlySpan<byte> clientPub, ulong timestamp)
    {
        if (!ProtoReader.TryReadFields(response, out var fields)) return false;
        if (!fields.TryGetBytes(1, out var encSecrets) || !fields.TryGetBytes(2, out var signature) ||
            !fields.TryGetBytes(3, out var serverPub)) return false;

        var input = new ProtoWriter();
        input.WriteRaw(clientPub);
        input.WriteRaw(serverPub);
        input.WriteRaw(encSecrets);
        Span<byte> be8 = stackalloc byte[8];
        BinaryPrimitives.WriteUInt64BigEndian(be8, timestamp);
        input.WriteRaw(be8);

        try
        {
            using var ecdsa = ECDsa.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = ServerVerifyPublicKey[1..33].ToArray(),
                    Y = ServerVerifyPublicKey[33..65].ToArray()
                }
            });
            return ecdsa.VerifyData(input.ToArray(), signature.ToArray(), HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private static byte[] EncryptGcm(ReadOnlySpan<byte> key, ReadOnlySpan<byte> plain)
    {
        byte[] iv = RandomNumberGenerator.GetBytes(12);
        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[16];
        aes.Encrypt(iv, plain, cipher, tag);

        var result = new byte[12 + plain.Length + 16];
        iv.CopyTo(result, 0);
        cipher.CopyTo(result, 12);
        tag.CopyTo(result, 12 + plain.Length);
        return result;
    }
}

internal sealed class MsfNgSessionKeys(byte[] secret1, byte[] secret2, long expirySeconds)
{
    public byte[] Secret1 { get; } = secret1;

    public byte[] Secret2 { get; } = secret2;

    public long ExpirySeconds { get; } = expirySeconds;
}

internal sealed class ProtoWriter
{
    private byte[] _buffer = new byte[256];

    private int _length;

    public void WriteVarInt(int field, ulong value)
    {
        WriteTag(field, WireType.VarInt);
        WriteVarIntRaw(value);
    }

    public void WriteFixed32(int field, uint value)
    {
        WriteTag(field, WireType.Fixed32);
        EnsureCapacity(4);
        _buffer[_length++] = (byte)value;
        _buffer[_length++] = (byte)(value >> 8);
        _buffer[_length++] = (byte)(value >> 16);
        _buffer[_length++] = (byte)(value >> 24);
    }

    public void WriteFixed64(int field, ulong value)
    {
        WriteTag(field, WireType.Fixed64);
        EnsureCapacity(8);
        for (int i = 0; i < 8; i++)
        {
            _buffer[_length++] = (byte)(value >> (8 * i));
        }
    }

    public void WriteBytes(int field, ReadOnlySpan<byte> value)
    {
        WriteTag(field, WireType.LengthDelimited);
        WriteVarIntRaw((uint)value.Length);
        WriteRaw(value);
    }

    public void WriteRaw(ReadOnlySpan<byte> value)
    {
        EnsureCapacity(value.Length);
        value.CopyTo(_buffer.AsSpan(_length));
        _length += value.Length;
    }

    public byte[] ToArray() => _buffer[.._length];

    private void EnsureCapacity(int additional)
    {
        if (_length + additional <= _buffer.Length) return;
        int capacity = _buffer.Length * 2;
        while (capacity < _length + additional) capacity *= 2;
        Array.Resize(ref _buffer, capacity);
    }

    private void WriteTag(int field, WireType type) => WriteVarIntRaw((uint)(field << 3) | (uint)type);

    private void WriteVarIntRaw(ulong value)
    {
        EnsureCapacity(10);
        while (value >= 0x80)
        {
            _buffer[_length++] = (byte)(value | 0x80);
            value >>= 7;
        }
        _buffer[_length++] = (byte)value;
    }
}

internal enum WireType : uint
{
    VarInt = 0,
    Fixed64 = 1,
    LengthDelimited = 2,
    Fixed32 = 5,
}

internal enum TsEncoding
{
    VarInt,
    Fixed64,
    Fixed32,
}

internal static class ProtoReader
{
    public static bool TryReadFields(ReadOnlySpan<byte> data, out ProtoFields fields)
    {
        fields = new ProtoFields();
        int position = 0;

        while (position < data.Length)
        {
            if (!TryReadVarInt(data, ref position, out var tag)) return false;

            int field = (int)(tag >> 3);
            var wireType = (WireType)(tag & 0x07);

            switch (wireType)
            {
                case WireType.VarInt:
                    if (!TryReadVarInt(data, ref position, out var varint)) return false;
                    fields.Add(field, new FieldValue(WireType.VarInt, varint, default));
                    break;
                case WireType.LengthDelimited:
                    if (!TryReadVarInt(data, ref position, out var length) || length > (uint)(data.Length - position)) return false;
                    fields.Add(field, new FieldValue(WireType.LengthDelimited, 0, data.Slice(position, (int)length).ToArray()));
                    position += (int)length;
                    break;
                case WireType.Fixed64:
                    if (position + 8 > data.Length) return false;
                    fields.Add(field, new FieldValue(WireType.Fixed64, BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(position)), default));
                    position += 8;
                    break;
                case WireType.Fixed32:
                    if (position + 4 > data.Length) return false;
                    fields.Add(field, new FieldValue(WireType.Fixed32, BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(position)), default));
                    position += 4;
                    break;
                default:
                    return false;
            }
        }

        return true;
    }

    private static bool TryReadVarInt(ReadOnlySpan<byte> data, ref int position, out ulong value)
    {
        value = 0;
        int shift = 0;
        while (position < data.Length)
        {
            byte b = data[position++];
            value |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0) return true;
            shift += 7;
            if (shift >= 64) return false;
        }
        return false;
    }

}

internal sealed class ProtoFields
{
    private readonly Dictionary<int, List<FieldValue>> _fields = [];

    public void Add(int field, FieldValue value)
    {
        if (!_fields.TryGetValue(field, out var list)) _fields[field] = list = [];
        list.Add(value);
    }

    public bool TryGetBytes(int field, out ReadOnlySpan<byte> value)
    {
        value = default;
        if (_fields.TryGetValue(field, out var list))
        {
            foreach (var candidate in list)
            {
                if (candidate.Type == WireType.LengthDelimited)
                {
                    value = candidate.Bytes!;
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryGetVarInt(int field, out ulong value)
    {
        value = 0;
        if (_fields.TryGetValue(field, out var list))
        {
            foreach (var candidate in list)
            {
                if (candidate.Type == WireType.VarInt)
                {
                    value = candidate.Number;
                    return true;
                }
            }
        }
        return false;
    }
}

internal readonly record struct FieldValue(WireType Type, ulong Number, byte[]? Bytes);
