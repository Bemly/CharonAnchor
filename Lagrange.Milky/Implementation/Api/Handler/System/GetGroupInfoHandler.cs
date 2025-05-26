using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Exception;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_info")]
public class GetGroupInfoHandler(BotContext bot, Converter converter) : IApiHandler<GetGroupInfoParameter, GetGroupInfoResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<GetGroupInfoResult> HandleAsync(GetGroupInfoParameter parameter, CancellationToken token)
    {
        var group = (await _bot.FetchGroups(parameter.NoCache ?? false))
            .FirstOrDefault(group => group.Uin == parameter.GroupId)
            ?? throw new ApiException(-1, "group not found");

        return new GetGroupInfoResult { Group = _converter.Group(group) };
    }
}

public class GetGroupInfoParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; } // false
}

public class GetGroupInfoResult
{
    [JsonPropertyName("group")]
    public required Entity.Group Group { get; init; }
}