using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Entity;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_group_member_list")]
public class GetGroupMemberListHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetGroupMemberListParameter, GetGroupMemberListResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetGroupMemberListResult> HandleAsync(GetGroupMemberListParameter parameter, CancellationToken token)
    {
        var members = await Task.WhenAll((await _bot.FetchMembers(parameter.GroupId, parameter.NoCache))
            .Select(member => _convert.GroupMemberAsync(member, token)));

        return new GetGroupMemberListResult(members);
    }
}

public class GetGroupMemberListParameter(long groupId, bool noCache = false)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonPropertyName("no_cache")]
    public bool NoCache { get; } = noCache;
}

public class GetGroupMemberListResult(IEnumerable<GroupMember> members)
{
    [JsonPropertyName("members")]
    public IEnumerable<GroupMember> Members { get; } = members;
}