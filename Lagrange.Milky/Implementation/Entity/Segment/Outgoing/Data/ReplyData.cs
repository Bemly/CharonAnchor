using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Outgoing.Data;

public class OutgoingReplyData
{
    [JsonPropertyName("message_seq")]
    public required long MessageSeq { get; init; }

    [JsonPropertyName("client_seq")]
    public long? ClientSeq { get; init; }
}