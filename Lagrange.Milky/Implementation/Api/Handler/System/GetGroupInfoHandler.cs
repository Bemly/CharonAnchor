using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Exception;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_info")]
public class GetGroupInfoHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetGroupInfoParameter, GetGroupInfoResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetGroupInfoResult> HandleAsync(GetGroupInfoParameter parameter, CancellationToken token)
    {
        var group = (await _bot.FetchGroups(parameter.NoCache))
            .FirstOrDefault(group => group.Uin == parameter.GroupId)
            ?? throw new ApiException(-1, "group not found");

        return new GetGroupInfoResult(_convert.Group(group));
    }
}

public class GetGroupInfoParameter(long groupId, bool noCache = false)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonPropertyName("no_cache")]
    public bool NoCache { get; } = noCache;
}

public class GetGroupInfoResult(Entity.Group group)
{
    [JsonPropertyName("group")]
    public Entity.Group Group { get; } = group;
}