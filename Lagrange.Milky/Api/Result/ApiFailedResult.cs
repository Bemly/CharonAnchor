using System.Text.Json.Serialization;

namespace Lagrange.Milky.Api.Result;

public class ApiFailedResult(long retcode, string message)
{
    [JsonPropertyName("status")]
    public string Status => "failed";

    [JsonPropertyName("retcode")]
    public long Retcode { get; } = retcode;

    [JsonPropertyName("message")]
    public string Message { get; } = message;
}