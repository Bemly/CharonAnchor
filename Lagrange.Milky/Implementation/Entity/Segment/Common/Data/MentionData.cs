using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Common.Data;

public class MentionData
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }
}