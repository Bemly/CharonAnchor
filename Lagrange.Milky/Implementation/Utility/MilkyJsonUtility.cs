using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Event;

namespace Lagrange.Milky.Implementation.Utility;

public static partial class MilkyJsonUtility
{
    // === api === 
    [JsonSerializable(typeof(IApiParameter))]
    [JsonSerializable(typeof(IApiResult))]

    // === event ===
    [JsonSerializable(typeof(IEvent))]
    private partial class MilkyJsonContext : JsonSerializerContext;

    public static byte[] SerializeToUtf8Bytes(Type type, object? value)
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, type, MilkyJsonContext.Default);
    }

    public static object? Deserialize(Type type, Stream json)
    {
        return JsonSerializer.Deserialize(json, type, MilkyJsonContext.Default);
    }
}
