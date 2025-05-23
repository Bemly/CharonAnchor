using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Incoming.Data;

public class IncomingImageData
{
    [JsonPropertyName("resource_id")]
    public required string ResourceId { get; init; }

    [JsonPropertyName("summary")]
    public required string Summary { get; init; }

    [JsonPropertyName("sub_type")]
    public required string SubType { get; init; }
}