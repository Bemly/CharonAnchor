using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Message.Entity;

public class AtSegment(long at, string? name) : ISegment
{
    [JsonPropertyName("qq")] public string At { get; set; } = at.ToString();
    
    [JsonPropertyName("name")] public string? Name { get; set; } = name;
}