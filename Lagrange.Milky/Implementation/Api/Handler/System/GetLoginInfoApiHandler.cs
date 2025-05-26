using Lagrange.Core;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Api.Result.Data;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_login_info")]
public class GetLoginInfoApiHandler(BotContext bot) : IEmptyApiHandler
{
    private readonly BotContext _bot = bot;

    public Task<IApiResult> HandleAsync(CancellationToken token)
    {
        if (_bot.BotInfo == null) return Task.FromResult(IApiResult.Failed(-1, "login info cannot be obtained"));

        return Task.FromResult(IApiResult.Ok(new GetLoginInfoResultData
        {
            Uin = _bot.BotUin,
            Nickname = _bot.BotInfo.Name,
        }));
    }
}