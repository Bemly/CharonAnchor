using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class Group(long groupId, string groupName, long memberCount, long maxMemberCount)
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("group_name")]
    public string GroupName { get; } = groupName;

    [JsonPropertyName("member_count")]
    public long MemberCount { get; } = memberCount;

    [JsonPropertyName("max_member_count")]
    public long MaxMemberCount { get; } = maxMemberCount;
}