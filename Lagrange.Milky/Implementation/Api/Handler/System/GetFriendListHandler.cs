using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_friend_list")]
public class GetFriendListHandler(BotContext bot, Converter converter) : IApiHandler<GetFriendListParameter, GetFriendListResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<GetFriendListResult> HandleAsync(GetFriendListParameter parameter, CancellationToken token)
    {
        return new GetFriendListResult
        {
            Friends = (await _bot.FetchFriends(parameter.NoCache ?? false)).Select(_converter.Friend)
        };
    }
}

public class GetFriendListParameter
{
    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; } // false
}

public class GetFriendListResult
{
    [JsonPropertyName("friends")]
    public required IEnumerable<Entity.Friend> Friends { get; init; }
}