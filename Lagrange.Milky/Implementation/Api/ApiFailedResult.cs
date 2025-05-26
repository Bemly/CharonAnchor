using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api;

public class ApiFailedResult
{
    [JsonPropertyName("status")]
    public static string Status => "failed";

    [JsonPropertyName("retcode")]
    public required long Retcode { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}