using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Entity.Action;

[Serializable]
public class OneBotMessageResponse(int messageId)
{
    [JsonPropertyName("message_id")] public int MessageId { get; set; } = messageId;
}