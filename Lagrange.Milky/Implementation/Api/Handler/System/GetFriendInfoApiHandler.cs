using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_friend_info")]
public class GetFriendInfoApiHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetFriendInfoApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<IApiResult> HandleAsync(GetFriendInfoApiParameter parameter, CancellationToken token)
    {
        var friend = (await _bot.FetchFriends(parameter.NoCache ?? false)).FirstOrDefault();
        if (friend == null) return IApiResult.Failed(-1, "friend not found");

        return IApiResult.Ok(_convert.Friend(friend));
    }
}
