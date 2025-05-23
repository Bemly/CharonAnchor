using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Event;

public class Event<TData>(string type) : IEvent
{
    [JsonPropertyName("time")]
    public required long Time { get; init; }

    [JsonPropertyName("self_id")]
    public required long SelfId { get; init; }

    [JsonPropertyName("event_type")]
    public string EventType { get; } = type;

    object? IEvent.Data => Data;
    [JsonPropertyName("data")]
    public required TData Data { get; init; }
}

