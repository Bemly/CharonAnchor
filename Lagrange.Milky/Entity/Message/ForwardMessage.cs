using System.Text.Json.Serialization;
using Lagrange.Milky.Entity.Segment;

namespace Lagrange.Milky.Entity.Message;

public class ForwardOutgoingMessage(long userId, string name, IReadOnlyList<IOutgoingSegment> segments)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonRequired]
    [JsonPropertyName("name")]
    public string Name { get; init; } = name;

    [JsonRequired]
    [JsonPropertyName("segments")]
    public IReadOnlyList<IOutgoingSegment> Segments { get; init; } = segments;
}