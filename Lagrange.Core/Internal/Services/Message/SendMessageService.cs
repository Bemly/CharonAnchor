using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<SendMessageEventReq>(Protocols.All)]
[Service("MessageSvc.PbSendMsg")]
internal class SendMessageService : BaseService<SendMessageEventReq, SendMessageEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(SendMessageEventReq input, BotContext context)
    {
        var payload = context.EventContext.GetLogic<MessagingLogic>().Build(input.Message);
        return new ValueTask<ReadOnlyMemory<byte>>(payload);
    }

    protected override ValueTask<SendMessageEventResp> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var response = ProtoHelper.Deserialize<PbSendMsgResp>(input.Span);
        int sequence = response.ClientSequence == 0 ? response.Sequence : response.ClientSequence;
        return new ValueTask<SendMessageEventResp>(new SendMessageEventResp(response.Result, response.SendTime, sequence));
    }
}