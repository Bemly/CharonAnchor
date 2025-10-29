using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Message;
using Lagrange.Milky.Api.Exception;
using Lagrange.Milky.Cache;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("recall_private_message")]
public class RecallPrivateMessageHandler(BotContext bot, MessageCache cache) : IEmptyResultApiHandler<RecallPrivateMessageParameter>
{
    private readonly BotContext _bot = bot;
    private readonly MessageCache _cache = cache;

    public async Task HandleAsync(RecallPrivateMessageParameter parameter, CancellationToken token)
    {
        var message = await _cache.GetMessageAsync(
            MessageType.Private,
            parameter.UserId,
            (ulong)parameter.MessageSeq,
            token
        );
        if (message == null) throw new ApiException(-2, "message not found");
        await _bot.RecallMessage(message);
    }
}

public class RecallPrivateMessageParameter(long userId, long messageSeq)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonRequired]
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; init; } = messageSeq;
}