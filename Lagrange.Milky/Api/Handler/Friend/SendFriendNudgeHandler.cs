using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Friend;

[Api("send_friend_nudge")]
public class SendFriendNudgeHandler(BotContext bot) : IApiHandler<SendFriendNudgeParameter, object>
{
    private readonly BotContext _bot = bot;

    public async Task<object> HandleAsync(SendFriendNudgeParameter parameter, CancellationToken token)
    {
        await _bot.SendFriendNudge(parameter.UserId, parameter.IsSelf ? _bot.BotUin : parameter.UserId);

        return new object();
    }
}

public class SendFriendNudgeParameter(long userId, bool isSelf = false)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("is_self")]
    public bool IsSelf { get; } = isSelf;
}