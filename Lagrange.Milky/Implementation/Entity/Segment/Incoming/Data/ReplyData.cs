using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Incoming.Data;

public class IncomingReplyData
{
    [JsonPropertyName("message_seq")]
    public required long MessageSeq { get; init; }
}