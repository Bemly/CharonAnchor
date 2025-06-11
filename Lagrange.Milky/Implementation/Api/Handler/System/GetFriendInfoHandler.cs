using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Exception;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_friend_info")]
public class GetFriendInfoHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetFriendInfoParameter, GetFriendInfoResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetFriendInfoResult> HandleAsync(GetFriendInfoParameter parameter, CancellationToken token)
    {
        var friend = (await _bot.FetchFriends(parameter.NoCache))
            .FirstOrDefault(friend => friend.Uin == parameter.UserId)
            ?? throw new ApiException(-1, "friend not found");

        return new GetFriendInfoResult(_convert.Friend(friend));
    }
}

public class GetFriendInfoParameter(long userId, bool noCache = false)
{
    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("no_cache")]
    public bool NoCache { get; } = noCache;
}

public class GetFriendInfoResult(Entity.Friend friend)
{
    [JsonPropertyName("friend")]
    public Entity.Friend Friend { get; } = friend;
}