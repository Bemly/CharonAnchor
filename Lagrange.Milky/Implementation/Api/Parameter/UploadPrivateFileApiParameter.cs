using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class UploadPrivateFileApiParameter : IApiParameter
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("file_uri")]
    public required string FileUri { get; init; }
}