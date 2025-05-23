using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_group_list")]
public class GetGroupListApiHandler(BotContext bot, EntityConvert @event) : IApiHandler<GetGroupListApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _event = @event;

    public async Task<IApiResult> HandleAsync(GetGroupListApiParameter parameter, CancellationToken token)
    {
        return IApiResult.Ok((await _bot.FetchGroups(parameter.NoCache ?? false)).Select(_event.Group));
    }
}