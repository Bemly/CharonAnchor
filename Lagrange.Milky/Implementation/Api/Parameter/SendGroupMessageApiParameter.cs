using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Entity.Segment.Outgoing;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class SendGroupMessageApiParameter : IApiParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("message")]
    public required IReadOnlyList<IOutgoingSegment> Message { get; init; }
}