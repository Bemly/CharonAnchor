using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class SendGroupNudgeApiParameter : IApiParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }
}