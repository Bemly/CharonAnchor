using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Message.Entity;

[Serializable]
public class RecordSegment(string url) : ISegment
{
    [JsonPropertyName("file")] public string File { get; set; } = url;

    [JsonPropertyName("url")] public string Url { get; set; } = url;
}