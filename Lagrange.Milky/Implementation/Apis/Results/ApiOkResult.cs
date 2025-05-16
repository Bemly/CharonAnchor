using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Apis.Results;

public class ApiOkResult<TData> : IApiResult
{
    [JsonPropertyName("status")]
    public string Status => "ok";

    [JsonPropertyName("retcode")]
    public long Retcode => 0;

    [JsonPropertyName("data")]
    public required TData Data { get; init; }
}