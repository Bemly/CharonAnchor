using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class SendFriendNudgeApiParameter : IApiParameter
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("is_self")]
    public required bool IsSelf { get; init; }
}