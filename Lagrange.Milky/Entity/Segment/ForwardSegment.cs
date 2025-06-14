using System.Text.Json.Serialization;
using Lagrange.Milky.Entity.Message;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class ForwardIncomingSegment(ForwardIncomingSegmentData data) : IncomingSegmentBase<ForwardIncomingSegmentData>(data)
{
    public ForwardIncomingSegment(string forwardId) : this(new ForwardIncomingSegmentData(forwardId)) { }
}

public class ForwardOutgoingSegment(ForwardOutgoingSegmentData data) : OutgoingSegmentBase<ForwardOutgoingSegmentData>(data) { }

public class ForwardIncomingSegmentData(string forwardId)
{
    [JsonPropertyName("forward_id")]
    public string ForwardId { get; } = forwardId;
}

public class ForwardOutgoingSegmentData(IReadOnlyList<ForwardOutgoingMessage> messages)
{
    [JsonRequired]
    [JsonPropertyName("messages")]
    public IReadOnlyList<ForwardOutgoingMessage> Messages { get; set; } = messages;
}