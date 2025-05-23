using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class ForwardSegment() : IncomingSegmentBase<ForwardData>("forward") { }

public class ForwardData
{
    [JsonPropertyName("forward_id")]
    public required string ForwardId { get; init; }
}