using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Outgoing.Data;

public class OutgoingVideoData
{
    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("thumb_uri")]
    public string? ThumbUri { get; init; }
}