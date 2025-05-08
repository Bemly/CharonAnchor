using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Message.Entity;

public class ImageSegment(string url, string filename, string summary, int subType) : ISegment
{
    [JsonPropertyName("file")] public string File { get; set; } = url;
    
    [JsonPropertyName("filename")] public string Filename { get; set; } = filename;
    
    [JsonPropertyName("url")] public string Url { get; set; }  = url;

    [JsonPropertyName("summary")] public string Summary { get; set; } = summary;
    
    [JsonPropertyName("subType")] public int SubType { get; set; } = subType;
}