using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Result.Data;

public class SendGroupMessageResultData
{
    [JsonPropertyName("message_seq")]
    public required long MessageSeq { get; init; }

    [JsonPropertyName("time")]
    public required long Time { get; init; }
}