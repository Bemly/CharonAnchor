using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment;

[method: JsonConstructor]
public class FaceIncomingSegment(FaceSegmentData data) : IncomingSegmentBase<FaceSegmentData>(data)
{
    public FaceIncomingSegment(string faceId) : this(new FaceSegmentData(faceId)) { }
}

public class FaceOutgoingSegment(FaceSegmentData data) : OutgoingSegmentBase<FaceSegmentData>(data) { }

public class FaceSegmentData(string faceId)
{
    [JsonPropertyName("face_id")]
    public string FaceId { get; } = faceId;
}