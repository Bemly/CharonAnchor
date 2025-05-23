using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Incoming.Segment;

public class TextSegment() : IncomingSegmentBase<TextData>("text") { }

public class TextData
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }
}