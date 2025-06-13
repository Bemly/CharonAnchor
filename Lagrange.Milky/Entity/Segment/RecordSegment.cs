using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class RecordIncomingSegment(RecordIncomingSegmentData data) : IncomingSegmentBase<RecordIncomingSegmentData>(data)
{
    public RecordIncomingSegment(string resourceId, string tempUrl, int duration) : this(new RecordIncomingSegmentData(resourceId, tempUrl, duration)) { }
}
public class RecordOutgoingSegment(RecordOutgoingSegmentData data) : OutgoingSegmentBase<RecordOutgoingSegmentData>(data) { }

public class RecordIncomingSegmentData(string resourceId, string tempUrl, int duration)
{
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; } = resourceId;

    [JsonPropertyName("temp_url")]
    public string TempUrl { get; } = tempUrl;

    [JsonPropertyName("duration")]
    public int Duration { get; } = duration;
}

public class RecordOutgoingSegmentData(string uri)
{
    [JsonRequired]
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = uri;
}