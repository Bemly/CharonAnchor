using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Api.Parameter;

public class GetGroupListApiParameter : IApiParameter
{
    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; }
}