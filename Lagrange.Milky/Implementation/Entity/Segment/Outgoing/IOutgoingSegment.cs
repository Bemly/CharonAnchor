using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Outgoing;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]

[JsonDerivedType(typeof(OutgoingTextSegment), typeDiscriminator: "text")]
[JsonDerivedType(typeof(OutgoingMentionSegment), typeDiscriminator: "mention")]
[JsonDerivedType(typeof(OutgoingMentionAllSegment), typeDiscriminator: "mention_all")]
[JsonDerivedType(typeof(OutgoingFaceSegment), typeDiscriminator: "face")]
[JsonDerivedType(typeof(OutgoingReplySegment), typeDiscriminator: "reply")]
[JsonDerivedType(typeof(OutgoingImageSegment), typeDiscriminator: "image")]
[JsonDerivedType(typeof(OutgoingRecordSegment), typeDiscriminator: "record")]
[JsonDerivedType(typeof(OutgoingVideoSegment), typeDiscriminator: "video")]
// TODO: [JsonDerivedType(typeof(OutgoingForwardSegment))]
public interface IOutgoingSegment
{
    string Type { get; }
    object? Data { get; }
}
