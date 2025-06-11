using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment;


[method: JsonConstructor]
public class TextIncomingSegment(TextSegmentData data) : IncomingSegmentBase<TextSegmentData>(data)
{
    public TextIncomingSegment(string text) : this(new TextSegmentData(text)) { }
}

public class TextOutgoingSegment(TextSegmentData data) : OutgoingSegmentBase<TextSegmentData>(data) { }

public class TextSegmentData(string text)
{
    [JsonRequired]
    [JsonPropertyName("text")]
    public string Text { get; init; } = text;
}