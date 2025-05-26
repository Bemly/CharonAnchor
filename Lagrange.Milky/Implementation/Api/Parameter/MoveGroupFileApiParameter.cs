using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class MoveGroupFileApiParameter : IApiParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("file_id")]
    public required string FileId { get; init; }

    [JsonPropertyName("target_folder_id")]
    public required string TargetFolderId { get; init; }
}