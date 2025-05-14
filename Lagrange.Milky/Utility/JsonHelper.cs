using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;

namespace Lagrange.Milky.Utility;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Default,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(ByteArrayConverter)])]

[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonObject))]

[JsonSerializable(typeof(BotKeystore))]
[JsonSerializable(typeof(BotQrCodeInfo))]

public partial class MilkyJsonContext : JsonSerializerContext;

public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return Convert.FromHexString(reader.GetString() ?? string.Empty);
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToHexString(value));
    }
}

public static class JsonHelper
{
    public static T? Deserialize<T>(string json) where T : class => 
        JsonSerializer.Deserialize(json, typeof(T), MilkyJsonContext.Default) as T;

    public static T? Deserialize<T>(this JsonNode? node) where T : class =>
        node.Deserialize(typeof(T), MilkyJsonContext.Default) as T;
    
    public static T? Deserialize<T>(this JsonElement element) where T : class =>
        element.Deserialize(typeof(T), MilkyJsonContext.Default) as T;

    public static object? Deserialize(this JsonElement? element, Type type) =>
        element?.Deserialize(type, MilkyJsonContext.Default);
    
    public static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, typeof(T), MilkyJsonContext.Default);
    
    public static byte[] SerializeToUtf8Bytes<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, typeof(T), MilkyJsonContext.Default);
}