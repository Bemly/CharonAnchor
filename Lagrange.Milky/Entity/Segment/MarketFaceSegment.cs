using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class MarketFaceIncomingSegment(MarketFaceSegmentData data) : IncomingSegmentBase<MarketFaceSegmentData>(data)
{
    public MarketFaceIncomingSegment(string url) : this(new MarketFaceSegmentData(url)) { }
}

public class MarketFaceSegmentData(string url)
{
    [JsonPropertyName("url")]
    public string Url { get; } = url;
}