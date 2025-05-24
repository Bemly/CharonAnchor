using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Incoming.Data;

public class IncomingRecordData
{
    [JsonPropertyName("resource_id")]
    public required string ResourceId { get; init; }

    [JsonPropertyName("duration")]
    public required int Duration { get; init; }
}