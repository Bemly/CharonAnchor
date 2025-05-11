using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Message.Entity;

public class VideoSegment(string url) : ISegment
{
    [JsonPropertyName("file")] public string File { get; set; } = url;
    
    [JsonPropertyName("url")] public string Url { get; set; }  = url;
}