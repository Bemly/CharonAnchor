using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_member_list")]
public class GetGroupMemberListApiHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetGroupMemberListApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<IApiResult> HandleAsync(GetGroupMemberListApiParameter parameter, CancellationToken token)
    {
        var members = await _bot.FetchMembers(parameter.GroupId, parameter.NoCache ?? false);
        return IApiResult.Ok(members.Select(_convert.GroupMember));
    }
}