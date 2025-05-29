using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Entity.Segment.Outgoing;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.Message;

[Api("send_private_message")]
public class SendPrivateMessageHandler(BotContext bot, Converter converter) : IApiHandler<SendPrivateMessageParameter, SendPrivateMessageResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<SendPrivateMessageResult> HandleAsync(SendPrivateMessageParameter parameter, CancellationToken token)
    {
        var result = await _bot.SendFriendMessage(
            parameter.UserId,
            await _converter.ToMessageChainAsync(parameter.Message, token)
        );

        return new SendPrivateMessageResult
        {
            MessageSeq = result.Sequence,
            Time = new DateTimeOffset(result.Time).ToUnixTimeSeconds(),
            ClientSeq = result.ClientSequence,
        };
    }
}

public class SendPrivateMessageParameter
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("message")]
    public required IReadOnlyList<IOutgoingSegment> Message { get; init; }
}

public class SendPrivateMessageResult
{
    [JsonPropertyName("message_seq")]
    public required long MessageSeq { get; init; }

    [JsonPropertyName("time")]
    public required long Time { get; init; }

    [JsonPropertyName("client_seq")]
    public required long ClientSeq { get; init; }
}