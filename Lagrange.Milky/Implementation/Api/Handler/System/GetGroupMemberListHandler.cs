using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Entity;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_member_list")]
public class GetGroupMemberListHandler(BotContext bot, Converter converter) : IApiHandler<GetGroupMemberListParameter, GetGroupMemberListResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<GetGroupMemberListResult> HandleAsync(GetGroupMemberListParameter parameter, CancellationToken token)
    {
        var members = await _bot.FetchMembers(parameter.GroupId, parameter.NoCache ?? false);

        return new GetGroupMemberListResult
        {
            Members = members.Select(_converter.GroupMember)
        };
    }
}

public class GetGroupMemberListParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; } // false
}

public class GetGroupMemberListResult
{
    [JsonPropertyName("members")]
    public required IEnumerable<GroupMember> Members { get; init; }
}