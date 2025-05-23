using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class RecordSegment() : IncomingSegmentBase<RecordData>("record") { }

public class RecordData
{
    [JsonPropertyName("resource_id")]
    public required string ResourceId { get; init; }

    [JsonPropertyName("duration")]
    public required int Summary { get; init; }
}