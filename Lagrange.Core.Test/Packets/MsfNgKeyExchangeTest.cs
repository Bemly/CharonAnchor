using System.Buffers.Binary;
using System.Security.Cryptography;
using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Test.Packets;

public class MsfNgKeyExchangeTest
{
    [Test]
    public void BuildRequest_ProducesWellFormedProtobuf()
    {
        var request = MsfNgKeyExchange.BuildRequest(out _, "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey", ReadOnlyMemory<byte>.Empty);

        Assert.That(ProtoReader.TryReadFields(request, out var fields), Is.True);
        Assert.That(fields.TryGetBytes(1, out var clientPub), Is.True);
        Assert.That(clientPub.Length, Is.EqualTo(65));
        Assert.That(clientPub[0], Is.EqualTo(0x04));
        Assert.That(fields.TryGetVarInt(2, out var flag) && flag == 1, Is.True);

        Assert.That(fields.TryGetBytes(3, out var payload), Is.True);
        Assert.That(payload.Length, Is.GreaterThan(28)); // IV(12) + inner pb + TAG(16)

        Assert.That(fields.TryGetVarInt(4, out var timestamp), Is.True);
        long unixNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Assert.That((long)timestamp, Is.InRange(unixNow - 60, unixNow + 60));

        Assert.That(fields.TryGetBytes(5, out var digestCipher), Is.True);
        Assert.That(digestCipher.Length, Is.EqualTo(32 + 28)); // SHA256 digest + IV + TAG
    }

    [Test]
    public void ParseResponse_ReturnsNullOnGarbage()
    {
        var request = MsfNgKeyExchange.BuildRequest(out var ephemeral, "x", ReadOnlyMemory<byte>.Empty);

        Assert.That(MsfNgKeyExchange.ParseResponse(ReadOnlySpan<byte>.Empty, ephemeral), Is.Null);
        Assert.That(MsfNgKeyExchange.ParseResponse(new byte[] { 0x08, 0x01 }, ephemeral), Is.Null);

        // well-formed protobuf but truncated AES payload
        var writer = new ProtoWriter();
        writer.WriteBytes(1, new byte[29]);
        writer.WriteBytes(2, new byte[64]);
        writer.WriteBytes(3, new byte[65]);
        Assert.That(MsfNgKeyExchange.ParseResponse(writer.ToArray(), ephemeral), Is.Null);
    }

    [Test]
    public void ProtoWriter_Reader_RoundTrip()
    {
        var writer = new ProtoWriter();
        writer.WriteVarInt(1, 300);
        writer.WriteBytes(2, new byte[] { 0xAA, 0xBB });
        writer.WriteVarInt(4, ulong.MaxValue);

        Assert.That(ProtoReader.TryReadFields(writer.ToArray(), out var fields), Is.True);
        Assert.That(fields.TryGetVarInt(1, out var v1) && v1 == 300, Is.True);
        Assert.That(fields.TryGetBytes(2, out var v2) && v2.ToArray().SequenceEqual(new byte[] { 0xAA, 0xBB }), Is.True);
        Assert.That(fields.TryGetVarInt(4, out var v4) && v4 == ulong.MaxValue, Is.True);
    }
}
