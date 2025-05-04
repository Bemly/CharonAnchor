using Lagrange.Core.Message;

namespace Lagrange.Core.Internal.Events.Message;

internal class SendMessageEventReq(BotMessage message) : ProtocolEvent
{
    public BotMessage Message { get; } = message;
}

internal class SendMessageEventResp(int result, long sendTime, int sequence) : ProtocolEvent
{
    public int Result { get; } = result;
    
    public long SendTime { get; } = sendTime;
    
    public int Sequence { get; } = sequence;
}