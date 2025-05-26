using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_list")]
public class GetGroupListHandler(BotContext bot, Converter converter) : IApiHandler<GetGroupListParameter, GetGroupListResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<GetGroupListResult> HandleAsync(GetGroupListParameter parameter, CancellationToken token)
    {
        return new GetGroupListResult
        {
            Groups = (await _bot.FetchGroups(parameter.NoCache ?? false)).Select(_converter.Group)
        };
    }
}

public class GetGroupListParameter
{
    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; } // false
}

public class GetGroupListResult
{
    [JsonPropertyName("groups")]
    public required IEnumerable<Entity.Group> Groups { get; init; }
}