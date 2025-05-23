using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Common.Data;

public class TextData
{
    [JsonPropertyName("text")]
    public required string Text { get; init; }
}