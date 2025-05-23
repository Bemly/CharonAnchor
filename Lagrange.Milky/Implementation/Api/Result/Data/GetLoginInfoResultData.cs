using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Result.Data;

public class GetLoginInfoResultData
{
    [JsonPropertyName("uin")]
    public required long Uin { get; init; }

    [JsonPropertyName("nickname")]
    public required string Nickname { get; init; }
}