using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.System;

[Api("get_friend_list")]
public class GetFriendListApiHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetFriendListApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<IApiResult> HandleAsync(GetFriendListApiParameter parameter, CancellationToken token)
    {
        return IApiResult.Ok((await _bot.FetchFriends(parameter.NoCache)).Select(_convert.Friend));
    }
}

public class GetFriendListApiParameter
{
    [JsonPropertyName("no_cache")]
    public bool NoCache { get; init; } = false;
}