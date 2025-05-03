using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Message.Entity;

public class TextSegment(string text) : ISegment
{
    [JsonPropertyName("text")] public string Text { get; set; } = text;
}