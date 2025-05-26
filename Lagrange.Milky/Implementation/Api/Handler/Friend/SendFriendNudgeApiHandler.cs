using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;

namespace Lagrange.Milky.Implementation.Api.Handler.Friend;

[Api("send_friend_nudge")]
public class SendFriendNudgeApiHandler(BotContext bot) : IApiHandler<SendFriendNudgeApiParameter>
{
    private readonly BotContext _bot = bot;

    public async Task<IApiResult> HandleAsync(SendFriendNudgeApiParameter parameter, CancellationToken token)
    {
        await _bot.SendFriendNudge(parameter.UserId, parameter.IsSelf ? _bot.BotUin : parameter.UserId);

        return IApiResult.Ok(new object { });
    }
}
