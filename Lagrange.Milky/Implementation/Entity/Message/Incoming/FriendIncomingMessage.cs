using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Message.Incoming;

public class FriendIncomingMessage() : IncomingMessageBase("friend")
{
    [JsonPropertyName("friend")]
    public required Friend Friend { get; init; }

    [JsonPropertyName("client_seq")]
    public required long ClientSeq { get; init; }
}