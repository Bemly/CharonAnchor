using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class ImageSegment() : IncomingSegmentBase<ImageData>("image") { }

public class ImageData
{
    [JsonPropertyName("resource_id")]
    public required string ResourceId { get; init; }

    [JsonPropertyName("summary")]
    public required string Summary { get; init; }

    [JsonPropertyName("sub_type")]
    public required string SubType { get; init; }
}