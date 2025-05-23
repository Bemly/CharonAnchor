using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Outgoing.Data;

public class OutgoingRecordData
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }
}