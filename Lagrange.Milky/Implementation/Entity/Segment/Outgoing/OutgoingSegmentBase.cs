using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Outgoing;

public abstract class OutgoingSegmentBase<TData>(string type) : IOutgoingSegment
{
    [JsonPropertyName("type")]
    public string Type { get; } = type;

    object? IOutgoingSegment.Data => Data;
    [JsonPropertyName("data")]
    public required TData Data { get; init; }
}