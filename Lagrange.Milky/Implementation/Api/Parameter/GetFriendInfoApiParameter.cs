using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class GetFriendInfoApiParameter : IApiParameter
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; }
}