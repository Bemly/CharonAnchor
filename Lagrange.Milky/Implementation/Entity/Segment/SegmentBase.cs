using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment;

public abstract class IncomingSegmentBase<TSegmentData>(TSegmentData data) : IIncomingSegment where TSegmentData : notnull
{
    object? IIncomingSegment.Data => Data;
    [JsonRequired]
    [JsonPropertyName("data")]
    public TSegmentData Data { get; init; } = data;
}

public abstract class OutgoingSegmentBase<TSegmentData>(TSegmentData data) : IOutgoingSegment where TSegmentData : notnull
{
    object? IOutgoingSegment.Data => Data;
    [JsonRequired]
    [JsonPropertyName("data")]
    public required TSegmentData Data { get; init; } = data;

}