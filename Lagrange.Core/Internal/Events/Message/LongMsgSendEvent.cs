using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;

namespace Lagrange.Core.Internal.Events.Message;

internal class LongMsgSendEventReq(BotContact contact, long? groupUin, List<BotMessage> messages) : ProtocolEvent
{
    public BotContact Contact { get; } = contact;
    
    public long? GroupUin { get; } = groupUin;
    
    public List<BotMessage> Messages { get; } = messages;
}

internal class LongMsgSendEventResp(string resId) : ProtocolEvent
{
    public string ResId { get; } = resId;
}