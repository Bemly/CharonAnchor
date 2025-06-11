using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment;

[method: JsonConstructor]
public class VideoIncomingSegment(VideoIncomingSegmentData data) : IncomingSegmentBase<VideoIncomingSegmentData>(data)
{
    public VideoIncomingSegment(string resourceId, string tempUrl) : this(new VideoIncomingSegmentData(resourceId, tempUrl)) { }
}

public class VideoOutgoingSegment(VideoOutgoingSegmentData data) : OutgoingSegmentBase<VideoOutgoingSegmentData>(data) { }

public class VideoIncomingSegmentData(string resourceId, string tempUrl)
{
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; } = resourceId;

    [JsonPropertyName("temp_url")]
    public string TempUrl { get; } = tempUrl;
}

public class VideoOutgoingSegmentData(string uri, string? thumbUri)
{
    [JsonRequired]
    [JsonPropertyName("uri")]
    public string Uri { get; init; } = uri;

    [JsonPropertyName("thumb_uri")]
    public string? ThumbUri { get; } = thumbUri;
}