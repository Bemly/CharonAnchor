using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

[JsonDerivedType(typeof(TextSegment))]
[JsonDerivedType(typeof(MentionSegment))]
[JsonDerivedType(typeof(MentionAllSegment))]
[JsonDerivedType(typeof(FaceSegment))]
[JsonDerivedType(typeof(ReplySegment))]
[JsonDerivedType(typeof(ImageSegment))]
[JsonDerivedType(typeof(RecordSegment))]
[JsonDerivedType(typeof(VideoSegment))]
[JsonDerivedType(typeof(ForwardSegment))]
[JsonDerivedType(typeof(MarketFaceSegment))]
[JsonDerivedType(typeof(LightAppSegment))]
[JsonDerivedType(typeof(XmlSegment))]
public interface IIncomingSegment
{
    string Type { get; }
    object? Data { get; }
}
