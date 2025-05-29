using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Milky.Implementation.Api.Exception;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_login_info")]
public class GetLoginInfoHandler(BotContext bot) : IApiHandler<object, GetLoginInfoResult>
{
    private readonly BotContext _bot = bot;

    public Task<GetLoginInfoResult> HandleAsync(object parameter, CancellationToken token)
    {
        if (_bot.BotInfo == null) throw new ApiException(-1, "login info is null");

        return Task.FromResult(new GetLoginInfoResult
        {
            Uin = _bot.BotUin,
            Nickname = _bot.BotInfo.Name,
        });
    }
}

public class GetLoginInfoResult
{
    [JsonPropertyName("uin")]
    public required long Uin { get; init; }

    [JsonPropertyName("nickname")]
    public required string Nickname { get; init; }
}