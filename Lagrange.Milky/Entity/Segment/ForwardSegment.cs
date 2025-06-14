using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ForwardIncomingSegment(ForwardIncomingSegmentData data) : IncomingSegmentBase<ForwardIncomingSegmentData>(data)
{
    public ForwardIncomingSegment(string forwardId) : this(new ForwardIncomingSegmentData(forwardId)) { }
}

public class ForwardIncomingSegmentData(string forwardId)
{
    [JsonPropertyName("forward_id")]
    public string ForwardId { get; } = forwardId;
}