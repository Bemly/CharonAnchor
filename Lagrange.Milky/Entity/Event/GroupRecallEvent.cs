using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class MessageRecallEvent(long time, long selfId, MessageRecallEventData data) : EventBase<MessageRecallEventData>(time, selfId, "message_recall", data) { }

public class MessageRecallEventData(string messageScene, long peerId, long messageSeq, long senderId, long operatorId, string displaySuffix)
{
    [JsonPropertyName("message_scene")]
    public string MessageScene { get; } = messageScene;

    [JsonPropertyName("peer_id")]
    public long PeerId { get; } = peerId;

    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; } = messageSeq;

    [JsonPropertyName("sender_id")]
    public long SenderId { get; } = senderId;

    [JsonPropertyName("operator_id")]
    public long OperatorId { get; } = operatorId;

    [JsonPropertyName("display_suffix")]
    public string DisplaySuffix { get; } = displaySuffix;
}