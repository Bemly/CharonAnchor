using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("send_group_nudge")]
public class SendGroupNudgeHandler(BotContext bot) : IEmptyResultApiHandler<SendGroupNudgeParameter>
{
    private readonly BotContext _bot = bot;

    public async Task HandleAsync(SendGroupNudgeParameter parameter, CancellationToken token)
    {
        await _bot.SendGroupNudge(parameter.GroupId, parameter.UserId);
    }
}

public class SendGroupNudgeParameter(long groupId, long userId)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;
}