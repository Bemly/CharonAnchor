using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity;

public class Group
{

    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("member_count")]
    public required long MemberCount { get; init; }

    [JsonPropertyName("max_member_count")]
    public required long MaxMemberCount { get; init; }
}