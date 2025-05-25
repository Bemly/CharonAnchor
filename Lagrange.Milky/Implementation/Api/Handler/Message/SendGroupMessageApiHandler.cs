using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Api.Result.Data;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.Message;

[Api("send_group_message")]
public class SendGroupMessageApiHandler(BotContext bot, EntityConvert convert) : IApiHandler<SendGroupMessageApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<IApiResult> HandleAsync(SendGroupMessageApiParameter parameter, CancellationToken token)
    {
        var result = await _bot.SendGroupMessage(
            await _convert.ToMessageChainAsync(parameter.Message, token),
            parameter.GroupId
        );

        return IApiResult.Ok(new SendGroupMessageResultData
        {
            MessageSeq = result.Sequence,
            Time = new DateTimeOffset(result.Time).ToUnixTimeSeconds(),
        });
    }
}