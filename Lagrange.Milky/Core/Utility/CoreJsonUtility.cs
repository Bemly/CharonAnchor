using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;

namespace Lagrange.Milky.Core.Utility;

public static partial class CoreJsonUtility
{
    // BotContext
    [JsonSerializable(typeof(BotKeystore))]
    [JsonSerializable(typeof(BotAppInfo))]

    // Signer
    [JsonSerializable(typeof(PcSecSignRequest))]
    [JsonSerializable(typeof(PcSecSignResponse))]
    [JsonSerializable(typeof(PcSecSignResponseValue))]
    [JsonSerializable(typeof(AndroidSecSignRequest))]
    [JsonSerializable(typeof(AndroidEnergyRequest))]
    [JsonSerializable(typeof(AndroidDebugXwidRequest))]
    [JsonSerializable(typeof(AndroidSignerResponse<AndroidSecSignResponseData>))]
    [JsonSerializable(typeof(AndroidSignerResponse<string>))]
    private partial class CoreJsonContext : JsonSerializerContext;

    public static string Serialize<T>(T value) where T : class
    {
        return JsonSerializer.Serialize(value, typeof(T), CoreJsonContext.Default);
    }

    public static byte[] SerializeToUtf8Bytes<T>(T value) where T : class
    {
        return JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), CoreJsonContext.Default);
    }

    public static T? Deserialize<T>(byte[] json) where T : class
    {
        return JsonSerializer.Deserialize(json, typeof(T), CoreJsonContext.Default) as T;
    }

    public static T? Deserialize<T>(Stream json) where T : class
    {
        return JsonSerializer.Deserialize(json, typeof(T), CoreJsonContext.Default) as T;
    }
}
