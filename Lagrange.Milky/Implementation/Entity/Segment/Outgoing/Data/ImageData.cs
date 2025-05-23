using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Outgoing.Data;

public class OutgoingImageData
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    [JsonPropertyName("sub_type")]
    public required string SubType { get; init; }
}