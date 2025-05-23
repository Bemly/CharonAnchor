using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class GetGroupMemberListApiParameter : IApiParameter
{
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; }

    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; }
}