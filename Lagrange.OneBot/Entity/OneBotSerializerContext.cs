using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Entity.Message;
using Lagrange.OneBot.Entity.Meta;
using Lagrange.OneBot.Message.Entity;

namespace Lagrange.OneBot.Entity;

[JsonSourceGenerationOptions(
    WriteIndented = true, 
    GenerationMode = JsonSourceGenerationMode.Default,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = [typeof(BooleanConverter), typeof(HexConverter)])]

[JsonSerializable(typeof(JsonNode))]
[JsonSerializable(typeof(JsonObject))]

[JsonSerializable(typeof(BotKeystore))]
[JsonSerializable(typeof(BotQrCodeInfo))]

[JsonSerializable(typeof(OneBotPrivateMessage))]
[JsonSerializable(typeof(OneBotGroupMessage))]
[JsonSerializable(typeof(OneBotSender))]
[JsonSerializable(typeof(OneBotGroupSender))]
[JsonSerializable(typeof(OneBotSegment))]
[JsonSerializable(typeof(TextSegment))]

[JsonSerializable(typeof(OneBotEntityBase))]
[JsonSerializable(typeof(OneBotAction))]
[JsonSerializable(typeof(OneBotResult))]
[JsonSerializable(typeof(OneBotHeartBeat))]
[JsonSerializable(typeof(OneBotMeta))]
[JsonSerializable(typeof(OneBotStatus))]
[JsonSerializable(typeof(OneBotLifecycle))]

[JsonSerializable(typeof(OneBotMessage))]
[JsonSerializable(typeof(OneBotMessageResponse))]
[JsonSerializable(typeof(OneBotQrCodeRequest))]
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
            return Convert.FromHexString(reader.GetString() ?? string.Empty);
        }
        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToHexString(value));
    }
}