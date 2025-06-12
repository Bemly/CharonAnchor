using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Entity.Segment;

namespace Lagrange.Milky.Implementation.Entity.Message;

[JsonDerivedType(typeof(FriendMessage))]
[JsonDerivedType(typeof(GroupMessage))]
[JsonDerivedType(typeof(TempMessage))]
public abstract class MessageBase(long peerId, long messageSeq, long senderId, long time, IReadOnlyList<IIncomingSegment> segments, string messageScene)
{
    [JsonPropertyName("peer_id")]
    public long PeerId { get; } = peerId;

    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;

    [JsonPropertyName("sender_id")]
    public long SenderId { get; } = senderId;

    [JsonPropertyName("time")]
    public long Time { get; } = time;

    [JsonPropertyName("segments")]
    public IReadOnlyList<IIncomingSegment> Segments { get; } = segments;

    [JsonPropertyName("message_scene")]
    public string MessageScene { get; } = messageScene;
}