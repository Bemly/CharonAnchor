using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Message;

namespace Lagrange.Core.Internal.Events.Message;

internal class SendMessageEventReq(BotMessage message) : ProtocolEvent
{
    public BotMessage Message { get; } = message;
}

internal class SendFriendFileEventReq(BotFriend friend, FileUploadEventReq request, FileUploadEventResp response, int clientSequence, uint sequence) : ProtocolEvent
{
    public BotFriend Friend { get; } = friend;
    
    public FileUploadEventReq Request { get; } = request;
    
    public FileUploadEventResp Response { get; } = response;
    
    public int ClientSequence { get; } = clientSequence;
    
    public uint Sequence { get; } = sequence;
}

internal class SendMessageEventResp(int result, long sendTime, int sequence) : ProtocolEvent
{
    public int Result { get; } = result;
    
    public long SendTime { get; } = sendTime;
    
    public int Sequence { get; } = sequence;
}