using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

[JsonDerivedType(typeof(GroupJoinRequestNotification))]
[JsonDerivedType(typeof(GroupAdminChangeNotification))]
[JsonDerivedType(typeof(GroupKickNotification))]
[JsonDerivedType(typeof(GroupQuitNotification))]
[JsonDerivedType(typeof(GroupInvitedJoinRequestNotification))]
public abstract class GroupNotificationBase(string type, long groupId, long notificationSeq)
{
    [JsonPropertyName("type")]
    public string Type { get; } = type;

    [JsonPropertyName("group_id")]
    public long GroupId { get; } = groupId;

    [JsonPropertyName("notification_seq")]
    public long NotificationSeq { get; } = notificationSeq;
}