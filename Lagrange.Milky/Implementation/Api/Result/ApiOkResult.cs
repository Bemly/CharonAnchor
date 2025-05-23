using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Result;

public class ApiOkResult<TData> : IApiResult
{
    [JsonPropertyName("status")]
    public string Status { get; } = "ok";

    [JsonPropertyName("retcode")]
    public long Retcode { get; } = 0;

    [JsonPropertyName("data")]
    public required TData Data { get; init; }
}