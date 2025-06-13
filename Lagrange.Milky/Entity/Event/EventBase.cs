using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Event;

public abstract class EventBase<TEventData>(long time, long selfId, string eventType, TEventData data)
{
    [JsonPropertyName("time")]
    public long Time { get; } = time;

    [JsonPropertyName("self_id")]
    public long SelfId { get; } = selfId;

    [JsonPropertyName("event_type")]
    public string EventType { get; } = eventType;

    [JsonPropertyName("data")]
    public TEventData Data { get; } = data;
}
