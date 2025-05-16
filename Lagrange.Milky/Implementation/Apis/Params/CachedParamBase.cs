using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Apis.Params;

public class CachedParam
{
    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; }
}