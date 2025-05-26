using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api;

public class ApiOkResult
{
    [JsonPropertyName("status")]
    public static string Status => "ok";

    [JsonPropertyName("retcode")]
    public static long Retcode => 0;

    [JsonPropertyName("data")]
    public required object Data { get; init; }
}