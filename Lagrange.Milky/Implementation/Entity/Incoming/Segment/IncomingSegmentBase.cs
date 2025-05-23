using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public abstract class IncomingSegmentBase<TData>(string type) : IIncomingSegment
{
    [JsonPropertyName("type")]
    public string Type { get; } = type;

    object? IIncomingSegment.Data => Data;
    [JsonPropertyName("data")]
    public required TData Data { get; init; }
}