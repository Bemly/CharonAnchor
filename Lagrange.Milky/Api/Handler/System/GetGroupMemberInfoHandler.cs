using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;
using Lagrange.Milky.Entity;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_group_member_info")]
public class GetGroupMemberInfoHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetGroupMemberInfoParameter, GetGroupMemberInfoResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetGroupMemberInfoResult> HandleAsync(GetGroupMemberInfoParameter parameter, CancellationToken token)
    {
        var member = (await _bot.FetchMembers(parameter.GroupId, parameter.NoCache))
            .FirstOrDefault(member => member.Uin == parameter.UserId)
            ?? throw new ApiException(-1, "group member not found");

        return new GetGroupMemberInfoResult(_convert.GroupMember(member));
    }
}

public class GetGroupMemberInfoParameter(long groupId, long userId, bool noCache = false)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonPropertyName("no_cache")]
    public bool NoCache { get; } = noCache;
}

public class GetGroupMemberInfoResult(GroupMember member)
{
    [JsonPropertyName("member")]
    public GroupMember Member { get; } = member;
}