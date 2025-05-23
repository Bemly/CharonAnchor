using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class FaceSegment() : IncomingSegmentBase<FaceData>("face") { }

public class FaceData
{
    [JsonPropertyName("face_id")]
    public required string FaceId { get; init; }
}