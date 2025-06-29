using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Milky.Api.Exception;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_login_info")]
public class GetLoginInfoHandler(BotContext bot) : IEmptyParameterApiHandler<GetLoginInfoResult>
{
    private readonly BotContext _bot = bot;

    public Task<GetLoginInfoResult> HandleAsync(CancellationToken token)
    {
        if (_bot.BotInfo == null) throw new ApiException(-1, "login info is null");

        return Task.FromResult(new GetLoginInfoResult(_bot.BotUin, _bot.BotInfo.Name));
    }
}

public class GetLoginInfoResult(long uin, string nickname)
{
    [JsonPropertyName("uin")]
    public long Uin { get; } = uin;

    [JsonPropertyName("nickname")]
    public string Nickname { get; } = nickname;
}