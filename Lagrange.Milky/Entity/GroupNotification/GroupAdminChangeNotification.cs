using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupAdminChangeNotification(long groupId, long notificationSeq, long targetUserId, bool isSet, long operatorId) : GroupNotificationBase("admin_change", groupId, notificationSeq)
{
    [JsonPropertyName("target_user_id")]
    public long TargetUserId { get; } = targetUserId;

    [JsonPropertyName("is_set")]
    public bool IsSet { get; } = isSet;

    [JsonPropertyName("operator_id")]
    public long OperatorId { get; } = operatorId;
}