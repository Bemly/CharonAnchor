using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity;

public class Friend
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("qid")]
    public required string Qid { get; init; }

    [JsonPropertyName("nickname")]
    public required string Nickname { get; init; }

    [JsonPropertyName("remark")]
    public required string Remark { get; init; }

    [JsonPropertyName("category")]
    public required FriendCategory Category { get; init; }
}
