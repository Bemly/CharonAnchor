using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Incoming;

[JsonDerivedType(typeof(IncomingTextSegment))]
[JsonDerivedType(typeof(IncomingMentionSegment))]
[JsonDerivedType(typeof(IncomingMentionAllSegment))]
[JsonDerivedType(typeof(IncomingFaceSegment))]
[JsonDerivedType(typeof(IncomingReplySegment))]
[JsonDerivedType(typeof(IncomingImageSegment))]
[JsonDerivedType(typeof(IncomingRecordSegment))]
[JsonDerivedType(typeof(IncomingVideoSegment))]
[JsonDerivedType(typeof(IncomingForwardSegment))]
[JsonDerivedType(typeof(IncomingMarketFaceSegment))]
[JsonDerivedType(typeof(IncomingLightAppSegment))]
[JsonDerivedType(typeof(IncomingXmlSegment))]
public interface IIncomingSegment
{
    string Type { get; }
    object? Data { get; }
}
