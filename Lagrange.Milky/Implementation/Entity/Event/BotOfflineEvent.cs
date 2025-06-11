using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Event;

public class BotOfflineEvent(long time, long selfId, BotOfflineEventData data) : EventBase<BotOfflineEventData>(time, selfId, "bot_offline", data) { }

public class BotOfflineEventData(string reason)
{
    [JsonPropertyName("reason")]
    public string Reason { get; } = reason;
}