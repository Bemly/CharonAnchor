using System.Text.Json.Serialization;
using Lagrange.OneBot.Entity.Message;

namespace Lagrange.OneBot.Entity.Action;

public class OneBotMessage
{
    [JsonPropertyName("message_type")] public string MessageType { get; set; } = "";
    
    [JsonPropertyName("user_id")] public long? UserId { get; set; }
    
    [JsonPropertyName("group_id")] public long? GroupId { get; set; }
    
    [JsonPropertyName("auto_escape")] public bool? AutoEscape { get; set; }
    
    [JsonPropertyName("message")] public List<OneBotSegment> Messages { get; set; } = [];
}