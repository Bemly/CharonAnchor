using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class LightAppSegment() : IncomingSegmentBase<LightAppData>("light_app") { }

public class LightAppData
{
    [JsonPropertyName("app_name")]
    public required string AppName { get; init; }

    [JsonPropertyName("json_payload")]
    public required string JsonPayload { get; init; }
}