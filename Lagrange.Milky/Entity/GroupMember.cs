using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupMember(long groupId, long userId, string nickname, string card, string? title, string sex, int level, string role, long joinTime, long lastSentTime)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("user_id")]
    public long UserId { get; } = userId;

    [JsonPropertyName("nickname")]
    public string Nickname { get; } = nickname;

    [JsonPropertyName("card")]
    public string Card { get; } = card;

    [JsonPropertyName("title")]
    public string? Title { get; } = title;

    [JsonPropertyName("sex")]
    public string Sex { get; } = sex;

    [JsonPropertyName("level")]
    public int Level { get; } = level;

    [JsonPropertyName("role")]
    public string Role { get; } = role;

    [JsonPropertyName("join_time")]
    public long JoinTime { get; } = joinTime;

    [JsonPropertyName("last_sent_time")]
    public long LastSentTime { get; } = lastSentTime;
}