using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Exception;
using Lagrange.Milky.Implementation.Entity;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_member_info")]
public class GetGroupMemberInfoHandler(BotContext bot, Converter converter) : IApiHandler<GetGroupMemberInfoParameter, GetGroupMemberInfoResult>
{
    private readonly BotContext _bot = bot;
    private readonly Converter _converter = converter;

    public async Task<GetGroupMemberInfoResult> HandleAsync(GetGroupMemberInfoParameter parameter, CancellationToken token)
    {
        var member = (await _bot.FetchMembers(parameter.GroupId, parameter.NoCache ?? false))
            .FirstOrDefault(member => member.Uin == parameter.UserId)
            ?? throw new ApiException(-1, "group member not found");

        return new GetGroupMemberInfoResult
        {
            Member = _converter.GroupMember(member),
        };
    }
}

public class GetGroupMemberInfoParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }

    [JsonPropertyName("no_cache")]
    public bool? NoCache { get; init; } // false
}

public class GetGroupMemberInfoResult
{
    [JsonPropertyName("member")]
    public required GroupMember Member { get; init; }
}