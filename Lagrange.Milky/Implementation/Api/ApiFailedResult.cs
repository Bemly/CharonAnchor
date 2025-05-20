using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api;

public class ApiFailedResult : IApiResult
{
    [JsonPropertyName("status")]
    public string Status { get; } = "failed";

    [JsonPropertyName("retcode")]
    public required long Retcode { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}