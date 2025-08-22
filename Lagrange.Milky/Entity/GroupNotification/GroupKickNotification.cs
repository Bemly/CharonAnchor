using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupKickNotification(long groupId, long notificationSeq, long targetUserId, long operatorId) : GroupNotificationBase("kick", groupId, notificationSeq)
{
    [JsonPropertyName("target_user_id")]
    public long TargetUserId { get; } = targetUserId;

    [JsonPropertyName("operator_id")]
    public long OperatorId { get; } = operatorId;
}