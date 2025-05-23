using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Common.Data;

public class LightAppData
{
    [JsonPropertyName("app_name")]
    public required string AppName { get; init; }

    [JsonPropertyName("json_payload")]
    public required string JsonPayload { get; init; }
}