
using Lagrange.Core;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_login_info")]
public class GetLoginInfoApiHandler(BotContext bot) : IEmptyApiHandler
{
    private readonly BotContext _bot = bot;

    public Task<IApiResult> HandleAsync(CancellationToken token)
    {
        if (_bot.BotInfo == null) return Task.FromResult(IApiResult.Failed(-1, "login info cannot be obtained"));

        return Task.FromResult(IApiResult.Ok(new GetLoginInfoResult
        {
            Uin = _bot.BotUin,
            Nickname = _bot.BotInfo.Name,
        }));
    }
}

public class GetLoginInfoResult
{
    public required long Uin { get; init; }
    public required string Nickname { get; init; }
}