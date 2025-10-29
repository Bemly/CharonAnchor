
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("recall_group_message")]
public class RecallGroupMessageHandler(BotContext bot) : IEmptyResultApiHandler<RecallGroupMessageParameter>
{
    private readonly BotContext _bot = bot;

    public Task HandleAsync(RecallGroupMessageParameter parameter, CancellationToken token)
    {
        return _bot.RecallGroupMessage(parameter.GroupId, (ulong)parameter.MessageSeq);
    }
}

public class RecallGroupMessageParameter(long groupId, long messageSeq)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; init; } = messageSeq;
}