using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class MarketFaceSegment() : IncomingSegmentBase<MarketFaceData>("market_face") { }

public class MarketFaceData
{
    [JsonPropertyName("url")]
    public required string Url { get; init; }
}