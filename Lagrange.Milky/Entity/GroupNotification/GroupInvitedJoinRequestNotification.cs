using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupInvitedJoinRequestNotification(long groupId, long notificationSeq, long initiatorId, long targetUserId, string state, long? operatorId) : GroupNotificationBase("invited_join_request", groupId, notificationSeq)
{
    [JsonPropertyName("initiator_id")]
    public long InitiatorId { get; } = initiatorId;

    [JsonPropertyName("target_user_id")]
    public long TargetUserId { get; } = targetUserId;

    [JsonPropertyName("state")]
    public string State { get; } = state;

    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; } = operatorId;
}