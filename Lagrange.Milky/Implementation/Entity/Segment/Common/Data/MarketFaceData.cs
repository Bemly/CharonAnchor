using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Common.Data;

public class MarketFaceData
{
    [JsonPropertyName("url")]
    public required string Url { get; init; }
}