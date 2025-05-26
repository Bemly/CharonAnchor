using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;

namespace Lagrange.Milky.Implementation.Api.Handler.Group;

[Api("send_group_nudge")]
public class SendGroupNudgeApiHandler(BotContext bot) : IApiHandler<SendGroupNudgeApiParameter>
{
    private readonly BotContext _bot = bot;

    public async Task<IApiResult> HandleAsync(SendGroupNudgeApiParameter parameter, CancellationToken token)
    {
        await _bot.SendGroupNudge(parameter.GroupId, parameter.UserId);

        return IApiResult.Ok(new object { });
    }
}