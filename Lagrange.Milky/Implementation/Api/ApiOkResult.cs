using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api;

public class ApiOkResult(object data)
{
    [JsonPropertyName("status")]
    public string Status => "ok";

    [JsonPropertyName("retcode")]
    public long Retcode => 0;

    [JsonPropertyName("data")]
    public object Data { get; } = data;
}