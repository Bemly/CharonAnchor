using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Entity.Segment.Outgoing;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.Message;

[Api("send_group_message")]
public class SendGroupMessageHandler(BotContext bot, Converter converter) : IApiHandler<SendGroupMessageParameter, SendGroupMessageResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<SendGroupMessageResult> HandleAsync(SendGroupMessageParameter parameter, CancellationToken token)
    {
        var result = await _bot.SendGroupMessage(
            parameter.GroupId,
            await _converter.ToMessageChainAsync(parameter.Message, token)
        );

        return new SendGroupMessageResult
        {
            MessageSeq = result.Sequence,
            Time = new DateTimeOffset(result.Time).ToUnixTimeSeconds(),
        };
    }
}

public class SendGroupMessageParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("message")]
    public required IReadOnlyList<IOutgoingSegment> Message { get; init; }
}

public class SendGroupMessageResult
{
    [JsonPropertyName("message_seq")]
    public required long MessageSeq { get; init; }

    [JsonPropertyName("time")]
    public required long Time { get; init; }
}