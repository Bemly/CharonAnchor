using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity;

public class GroupJoinRequestNotification(long groupId, long notificationSeq, bool isFiltered, long initiatorId, string state, long? operatorId, string comment) : GroupNotificationBase("join_request", groupId, notificationSeq)
{
    [JsonPropertyName("is_filtered")]
    public bool IsFiltered { get; } = isFiltered;

    [JsonPropertyName("initiator_id")]
    public long InitiatorId { get; } = initiatorId;

    [JsonPropertyName("state")]
    public string State { get; } = state;

    [JsonPropertyName("operator_id")]
    public long? OperatorId { get; } = operatorId;

    [JsonPropertyName("comment")]
    public string Comment { get; } = comment;
}