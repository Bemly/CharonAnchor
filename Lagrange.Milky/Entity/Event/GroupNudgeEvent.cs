using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupNudgeEvent(long time, long selfId, GroupNudgeEventData data) : EventBase<GroupNudgeEventData>(time, selfId, "group_nudge", data) { }

public class GroupNudgeEventData(long groupID, long sender_id, long receiver_id)
{
    [JsonPropertyName("group_id")]
    public long GroupID { get; } = groupID;
    
    [JsonPropertyName("sender_id")]
    public long SenderID { get; } = sender_id;
    
    [JsonPropertyName("receiver_id")]
    public long ReceiverID { get; } = receiver_id;
}
