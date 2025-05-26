using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Exception;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_friend_info")]
public class GetFriendInfoHandler(BotContext bot, Converter converter) : IApiHandler<GetFriendInfoParameter, GetFriendInfoResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<GetFriendInfoResult> HandleAsync(GetFriendInfoParameter parameter, CancellationToken token)
    {
        var friend = (await _bot.FetchFriends(parameter.NoCache ?? false))
            .FirstOrDefault(friend => friend.Uin == parameter.UserId)
            ?? throw new ApiException(-1, "friend not found");

        return new GetFriendInfoResult { Friend = _converter.Friend(friend) };
    }
}

public class GetFriendInfoParameter
{
    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; } // false
}

public class GetFriendInfoResult
{
    [JsonPropertyName("friend")]
    public required Entity.Friend Friend { get; init; }
}