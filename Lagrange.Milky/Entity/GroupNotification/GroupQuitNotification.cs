using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupQuitNotification(long groupId, long notificationSeq, long targetUserId) : GroupNotificationBase("quit", groupId, notificationSeq)
{
    [JsonPropertyName("target_user_id")]
    public long TargetUserId { get; } = targetUserId;
}