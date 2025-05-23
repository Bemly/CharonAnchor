using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Entity.Segment.Incoming;

namespace Lagrange.Milky.Implementation.Entity.Message.Incoming;

[JsonDerivedType(typeof(FriendIncomingMessage))]
[JsonDerivedType(typeof(GroupIncomingMessage))]
[JsonDerivedType(typeof(TempIncomingMessage))]
public abstract class IncomingMessageBase(string type)
{
    [JsonPropertyName("peer_id")]
    public required long PeerId { get; init; }

    [JsonPropertyName("message_seq")]
    public required long MessageSeq { get; init; }

    [JsonPropertyName("sender_id")]
    public required long SenderId { get; init; }

    [JsonPropertyName("time")]
    public required long Time { get; init; }

    [JsonPropertyName("segments")]
    public required IReadOnlyList<IIncomingSegment> Segments { get; init; }

    [JsonPropertyName("message_scene")]
    public string MessageScene { get; } = type;
}