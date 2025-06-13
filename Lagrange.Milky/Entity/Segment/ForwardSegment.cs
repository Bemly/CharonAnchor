using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ForwardIncomingSegment(ForwardSegmentData data) : IncomingSegmentBase<ForwardSegmentData>(data)
{
    public ForwardIncomingSegment(string forwardId) : this(new ForwardSegmentData(forwardId)) { }
}

public class ForwardOutgoingSegment(ForwardSegmentData data) : OutgoingSegmentBase<ForwardSegmentData>(data) { }

public class ForwardSegmentData(string forwardId)
{
    [JsonPropertyName("forward_id")]
    public string ForwardId { get; } = forwardId;
}