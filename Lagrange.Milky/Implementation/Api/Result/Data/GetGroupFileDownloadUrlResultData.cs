using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Result.Data;

public class GetGroupFileDownloadUrlResultData
{
    [JsonPropertyName("download_url")]
    public required string DownloadUrl { get; init; }
}