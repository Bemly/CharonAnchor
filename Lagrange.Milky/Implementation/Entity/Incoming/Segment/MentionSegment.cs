using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class MentionSegment() : IncomingSegmentBase<MentionData>("mention") { }

public class MentionData
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }
}