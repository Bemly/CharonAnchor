using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Api.Result.Data;
using Lagrange.Milky.Implementation.Utility;

namespace Lagrange.Milky.Implementation.Api.Handler.Message;

[Api("send_private_message")]
public class SendPrivateMessageApiHandler(BotContext bot, EntityConvert entity) : IApiHandler<SendPrivateMessageApiParameter>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _entity = entity;

    public async Task<IApiResult> HandleAsync(SendPrivateMessageApiParameter parameter, CancellationToken token)
    {
        var result = await _bot.SendFriendMessage(
            await _entity.ToMessageChainAsync(parameter.Message, token),
            parameter.UserId
        );

        return IApiResult.Ok(new SendPrivateMessageResultData
        {
            MessageSeq = result.Sequence,
            Time = new DateTimeOffset(result.Time).ToUnixTimeSeconds(),
            ClientSeq = result.ClientSequence,
        });
    }
}