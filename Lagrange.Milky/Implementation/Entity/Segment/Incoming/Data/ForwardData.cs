using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Incoming.Data;

public class IncomingForwardData
{
    [JsonPropertyName("forward_id")]
    public required string ForwardId { get; init; }
}