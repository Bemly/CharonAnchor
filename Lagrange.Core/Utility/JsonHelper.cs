using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;

namespace Lagrange.Core.Utility;

internal static partial class JsonHelper
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
    [JsonSerializable(typeof(DefaultAndroidBotSignProvider.ResponseRoot<DefaultAndroidBotSignProvider.SignResponse>))]
    [JsonSerializable(typeof(DefaultAndroidBotSignProvider.ResponseRoot<string>))]
    [JsonSerializable(typeof(DefaultBotSignProvider.Root))]
    [JsonSerializable(typeof(DefaultBotSignProvider.Response))]
    [JsonSerializable(typeof(JsonObject))]
    private partial class CoreSerializerContext : JsonSerializerContext;
    
    public static T? Deserialize<T>(string json) where T : class=> 
        JsonSerializer.Deserialize(json, typeof(T), CoreSerializerContext.Default) as T;
    
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, typeof(T), CoreSerializerContext.Default);
}