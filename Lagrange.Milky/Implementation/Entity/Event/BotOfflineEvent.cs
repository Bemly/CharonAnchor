using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Event;

public class BotOfflineEvent
{
    [JsonPropertyName("reason")]
    public required string Reason { get; init; }
}