using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_friend_list")]
public class GetFriendListHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetFriendListParameter, GetFriendListResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetFriendListResult> HandleAsync(GetFriendListParameter parameter, CancellationToken token)
    {
        var friends = await _bot.FetchFriends(parameter.NoCache);

        return new GetFriendListResult(friends.Select(_convert.Friend));
    }
}

public class GetFriendListParameter(bool noCache = false)
{
    [JsonPropertyName("no_cache")]
    public bool NoCache { get; } = noCache;
}

public class GetFriendListResult(IEnumerable<Entity.Friend> friends)
{
    [JsonPropertyName("friends")]
    public IEnumerable<Entity.Friend> Friends { get; } = friends;
}