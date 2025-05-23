using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Entity.Segment.Outgoing;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class SendPrivateMessageApiParameter : IApiParameter
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("message")]
    public required IReadOnlyList<IOutgoingSegment> Message { get; init; }
}