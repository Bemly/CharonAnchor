using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Message.Incoming;

public class TempIncomingMessage() : IncomingMessageBase("temp")
{
    [JsonPropertyName("group")]
    public Group? Group { get; init; }
}