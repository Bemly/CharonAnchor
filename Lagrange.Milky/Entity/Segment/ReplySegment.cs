using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ReplyIncomingSegment(ReplySegmentData data) : IncomingSegmentBase<ReplySegmentData>(data)
{
    public ReplyIncomingSegment(long messageSeq) : this(new ReplySegmentData(messageSeq)) { }
}

public class ReplyOutgoingSegment(ReplySegmentData data) : OutgoingSegmentBase<ReplySegmentData>(data) { }

public class ReplySegmentData(long messageSeq)
{
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;
}