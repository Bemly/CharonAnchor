using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public class GroupNudgeEvent(long time, long selfId, GroupNudgeEventData data) : EventBase<GroupNudgeEventData>(time, selfId, "group_nudge", data) { }

public class GroupNudgeEventData(Int64 groupID, Int64 sender_id, Int64 receiver_id)
{
    [JsonPropertyName("group_id")]
    public Int64 GroupID { get; } = groupID;
    
    [JsonPropertyName("sender_id")]
    public Int64 SenderID { get; } = sender_id;
    
    [JsonPropertyName("receiver_id")]
    public Int64 ReceiverID { get; } = receiver_id;
}
