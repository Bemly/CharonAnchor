using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment;

[method: JsonConstructor]
public class LightAppIncomingSegment(LightAppSegmentData data) : IncomingSegmentBase<LightAppSegmentData>(data)
{
    public LightAppIncomingSegment(string appName, string jsonPayload) : this(new LightAppSegmentData(appName, jsonPayload)) { }
}

public class LightAppSegmentData(string appName, string jsonPayload)
{
    [JsonPropertyName("app_name")]
    public string AppName { get; } = appName;

    [JsonPropertyName("json_payload")]
    public string JsonPayload { get; } = jsonPayload;
}