using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_info")]
public class GetGroupInfoApiHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetGroupInfoApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<IApiResult> HandleAsync(GetGroupInfoApiParameter parameter, CancellationToken token)
    {
        var group = (await _bot.FetchGroups(parameter.NoCache ?? false))
            .FirstOrDefault(group => group.Uin == parameter.GroupId);
        if (group == null) return IApiResult.Failed(-1, "group not found");

        return IApiResult.Ok(_convert.Group(group));
    }
}