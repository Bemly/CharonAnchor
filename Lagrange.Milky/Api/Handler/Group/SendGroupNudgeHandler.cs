using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("send_group_nudge")]
public class SendGroupNudgeHandler(BotContext bot) : IApiHandler<SendGroupNudgeParameter, object>
{
    private readonly BotContext _bot = bot;

    public async Task<object> HandleAsync(SendGroupNudgeParameter parameter, CancellationToken token)
    {
        await _bot.SendGroupNudge(parameter.GroupId, parameter.UserId);

        return new object();
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