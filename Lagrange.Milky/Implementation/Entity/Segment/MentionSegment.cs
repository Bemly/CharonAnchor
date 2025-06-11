using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment;

[method: JsonConstructor]
public class MentionIncomingSegment(MentionSegmentData data) : IncomingSegmentBase<MentionSegmentData>(data)
{
    public MentionIncomingSegment(long userId) : this(new MentionSegmentData(userId)) { }
}

public class MentionOutgoingSegment(MentionSegmentData data) : OutgoingSegmentBase<MentionSegmentData>(data) { }

public class MentionSegmentData(long userId)
{
    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;
}