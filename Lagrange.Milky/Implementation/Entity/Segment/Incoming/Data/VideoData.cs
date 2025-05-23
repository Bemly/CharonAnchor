using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Incoming.Data;

public class IncomingVideoData
{
    [JsonPropertyName("resource_id")]
    public required string ResourceId { get; init; }
}