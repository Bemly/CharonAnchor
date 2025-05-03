using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.OneBot.Message;

namespace Lagrange.OneBot.Entity.Message;

public class OneBotSegment(string type, object segment)
{
    [JsonPropertyName("type")] public string Type { get; set; } = type;

    [JsonPropertyName("data")] public object Data { get; set; } = segment;
}