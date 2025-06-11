using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity;

public class Group(long groupId, string name, long memberCount, long maxMemberCount)
{

    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("name")]
    public string Name { get; } = name;

    [JsonPropertyName("member_count")]
    public long MemberCount { get; } = memberCount;

    [JsonPropertyName("max_member_count")]
    public long MaxMemberCount { get; } = maxMemberCount;
}