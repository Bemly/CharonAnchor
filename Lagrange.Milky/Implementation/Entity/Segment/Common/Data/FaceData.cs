using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Common.Data;

public class FaceData
{
    [JsonPropertyName("face_id")]
    public required string FaceId { get; init; }
}