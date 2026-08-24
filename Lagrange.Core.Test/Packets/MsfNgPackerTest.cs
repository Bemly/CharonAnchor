using System.Buffers.Binary;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Services;

namespace Lagrange.Core.Test.Packets;

public class MsfNgPackerTest
{
    private const string HeartbeatCommand = "Heartbeat.Alive";

    private static readonly byte[] CapturedHeartbeat = Convert.FromHexString(
        "0000007E0000000D00006238BF000000000400000064000000134865617274626561742E416C697665000000040000004962206238623934323930613937313165653130646161313030363462376435373533BA011D0A0F636C69656E745F636F6E6E5F736571120A31373837343139313539D001650000000800000004");

    private static readonly byte[] CapturedHeartbeatPong = Convert.FromHexString(
        "000000500000000D0000000000053000000039006238BF0000000000000004000000134865617274626561742E416C69766500000004000000000000000AA80100C801026A89DAE20000000800000004");

    private static MsfNgPacker CreatePacker()
    {
        var context = new BotContext(new BotConfig(), new BotKeystore
        {
            Uin = 3156037162,
            Guid = Convert.FromHexString("B8B94290A9711EE10DAA10064B7D5753")
        }, BotAppInfo.ProtocolToAppInfo[Protocols.Linux]);
        return new MsfNgPacker(context);
    }

    [Test]
    public void BuildHeartbeat_MatchesCapturedFrame()
    {
        var packer = CreatePacker();
        var sso = new BotSsoPacket(HeartbeatCommand, ReadOnlyMemory<byte>.Empty, 0x6238BF);
        var options = new ServiceAttribute(HeartbeatCommand, RequestType.Simple, EncryptType.NoEncrypt);

        var frame = packer.BuildProtocol13(sso, options);

        Assert.That(frame.Length, Is.EqualTo(CapturedHeartbeat.Length));
        Assert.That(frame.Span[..49].ToArray(), Is.EqualTo(CapturedHeartbeat[..49]));
        Assert.That(frame.Span[^8..].ToArray(), Is.EqualTo(CapturedHeartbeat[^8..]));

        var trace = frame.Span[49..^8];
        Assert.That(trace.Length, Is.EqualTo(69));
        Assert.That(trace[0], Is.EqualTo((byte)'b'));
        Assert.That(trace[1], Is.EqualTo((byte)' '));
        Assert.That(Encoding.ASCII.GetString(trace.Slice(2, 32)), Does.Match("^[0-9a-f]{32}$"));
        Assert.That(trace[34..37].ToArray(), Is.EqualTo(new byte[] { 0xBA, 0x01, 29 }));
        Assert.That(trace[37], Is.EqualTo(0x0A));
        Assert.That(trace[38], Is.EqualTo(15));
        Assert.That(Encoding.ASCII.GetString(trace.Slice(39, 15)), Is.EqualTo("client_conn_seq"));
        Assert.That(trace[54], Is.EqualTo(0x12));
        Assert.That(trace[55], Is.EqualTo(10));
        Assert.That(Encoding.ASCII.GetString(trace.Slice(56, 10)), Does.Match("^\\d{10}$"));
        Assert.That(trace[66..69].ToArray(), Is.EqualTo(new byte[] { 0xD0, 0x01, 0x65 }));
    }

    [Test]
    public void Parse_ReturnsNullForChannelLevelPong()
    {
        var packer = CreatePacker();

        Assert.That(packer.Parse(CapturedHeartbeatPong), Is.Null);
    }

    [Test]
    public void Parse_RejectsMalformedFrames()
    {
        var packer = CreatePacker();

        Assert.That(packer.Parse(ReadOnlySpan<byte>.Empty), Is.Null);

        var truncated = CapturedHeartbeat.AsSpan()[..20].ToArray();
        Assert.That(packer.Parse(truncated), Is.Null);

        var bogusVersion = (byte[])CapturedHeartbeat.Clone();
        BinaryPrimitives.WriteUInt32BigEndian(bogusVersion.AsSpan(4), 99);
        Assert.That(packer.Parse(bogusVersion), Is.Null);
    }

    [Test]
    public void BuildZeroKeyEncryptedFrame_HasExpectedEnvelope()
    {
        var packer = CreatePacker();
        var sso = new BotSsoPacket(HeartbeatCommand, ReadOnlyMemory<byte>.Empty, 0x6238BF);
        var options = new ServiceAttribute(HeartbeatCommand, RequestType.Simple, EncryptType.EncryptEmpty);

        var frame = packer.BuildProtocol13(sso, options);

        Assert.That(frame.Span[8], Is.EqualTo((byte)MsfNgEncrypt.ZeroKey));
        int cipherLength = BinaryPrimitives.ReadInt32BigEndian(frame.Span) - 18;
        Assert.That(cipherLength % 8, Is.Zero);
    }

    [Test]
    public void BuildSessionKeyFrame_WithoutKey_Throws()
    {
        var packer = CreatePacker();
        var sso = new BotSsoPacket(HeartbeatCommand, ReadOnlyMemory<byte>.Empty, 0x6238BF);
        var options = new ServiceAttribute(HeartbeatCommand, RequestType.Simple, EncryptType.EncryptD2Key);

        Assert.Throws<InvalidOperationException>(() => packer.BuildProtocol13(sso, options));
    }
}
