using System.Text.Json;
using Lagrange.OneBot.Entity;

namespace Lagrange.OneBot.Utility;

public static class JsonHelper
{
    public static T? Deserialize<T>(string json) where T : class => 
        JsonSerializer.Deserialize(json, typeof(T), OneBotSerializerContext.Default) as T;
    
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, typeof(T), OneBotSerializerContext.Default);
    
    public static byte[] SerializeToUtf8Bytes<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), OneBotSerializerContext.Default);
}