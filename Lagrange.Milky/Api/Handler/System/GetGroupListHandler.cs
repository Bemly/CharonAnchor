using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_group_list")]
public class GetGroupListHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetGroupListParameter, GetGroupListResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetGroupListResult> HandleAsync(GetGroupListParameter parameter, CancellationToken token)
    {
        var groups = (await _bot.FetchGroups(parameter.NoCache)).Select(_convert.Group);

        return new GetGroupListResult(groups);
    }
}

public class GetGroupListParameter(bool noCache = false)
{
    [JsonPropertyName("no_cache")]
    public bool NoCache { get; } = noCache;
}

public class GetGroupListResult(IEnumerable<Entity.Group> groups)
{
    [JsonPropertyName("groups")]
    public IEnumerable<Entity.Group> Groups { get; } = groups;
}