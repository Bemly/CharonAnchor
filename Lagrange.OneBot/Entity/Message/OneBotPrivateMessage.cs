using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Entity.Message;

[Serializable]
public class OneBotPrivateMessage(long selfId, OneBotSender sender, string subType, long time, List<OneBotSegment> message) : OneBotEntityBase(selfId, "message", time)
{
    [JsonPropertyName("message_type")] public string MessageType { get; set; } = "private";

    [JsonPropertyName("sub_type")] public string SubType { get; set; } = subType;

    [JsonPropertyName("message_id")] public int MessageId { get; set; }
    
    [JsonPropertyName("user_id")] public long UserId { get; set; }
    
    [JsonPropertyName("message")] public List<OneBotSegment> Message { get; set; } = message;

    [JsonPropertyName("raw_message")] public string RawMessage { get; set; } = string.Empty;

    [JsonPropertyName("font")] public int Font { get; set; }

    [JsonPropertyName("sender")] public OneBotSender Sender { get; set; } = sender;

    [JsonPropertyName("target_id")] public long TargetId { get; set; }
}