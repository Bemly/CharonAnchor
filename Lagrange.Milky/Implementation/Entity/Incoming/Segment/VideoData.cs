using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class VideoSegment() : IncomingSegmentBase<VideoData>("video") { }

public class VideoData
{
    [JsonPropertyName("resource_id")]
    public required string ResourceId { get; init; }
}