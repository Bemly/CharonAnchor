using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_member_info")]
public class GetGroupMemberInfoApiHandler(BotContext bot, EntityConvert entity) : IApiHandler<GetGroupMemberInfoApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _entity = entity;

    public async Task<IApiResult> HandleAsync(GetGroupMemberInfoApiParameter parameter, CancellationToken token)
    {
        var member = (await _bot.FetchMembers(parameter.GroupId, parameter.NoCache ?? false))
            .FirstOrDefault(member => member.Uin == parameter.UserId);
        if (member == null) return IApiResult.Failed(-1, "group member not found");

        return IApiResult.Ok(_entity.GroupMember(member));
    }
}