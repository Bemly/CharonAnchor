using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Entity.Meta;

namespace Lagrange.OneBot.Entity;

[JsonSourceGenerationOptions(
    WriteIndented = true, 
    GenerationMode = JsonSourceGenerationMode.Default,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(BooleanConverter), typeof(HexConverter)])]

[JsonSerializable(typeof(BotKeystore))]

[JsonSerializable(typeof(OneBotEntityBase))]
[JsonSerializable(typeof(OneBotAction))]
[JsonSerializable(typeof(OneBotResult))]
[JsonSerializable(typeof(OneBotHeartBeat))]
[JsonSerializable(typeof(OneBotMeta))]
[JsonSerializable(typeof(OneBotStatus))]
[JsonSerializable(typeof(OneBotLifecycle))]
public partial class OneBotSerializerContext : JsonSerializerContext;

public class BooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String when Utf8Parser.TryParse(reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan, out bool value, out _) => value,
            _ => throw new JsonException()
        };
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }
}

public class HexConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.TryGetBytesFromBase64(out var bytes)
                ? bytes 
                : Convert.FromHexString(reader.GetString() ?? string.Empty);
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToHexString(value));
    }
}