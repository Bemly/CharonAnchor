using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class ReplySegment() : IncomingSegmentBase<ReplyData>("reply") { }

public class ReplyData
{
    [JsonPropertyName("message_seq")]
    public required string MessageSeq { get; init; }
}