using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Message;

namespace Lagrange.Core.Common.Interface;

public static class MessageExt
{
    public static Task<BotMessage> SendFriendMessage(this BotContext context, MessageChain chain, long friendUin) 
        => context.EventContext.GetLogic<MessagingLogic>().SendFriendMessage(chain, friendUin);

    public static Task<BotMessage> SendGroupMessage(this BotContext context, MessageChain chain, long groupUin)
        => context.EventContext.GetLogic<MessagingLogic>().SendGroupMessage(chain, groupUin);

    public static Task<bool> SendFriendFile(this BotContext context, long targetUin, Stream fileStream, string? fileName = null)
        => context.EventContext.GetLogic<OperationLogic>().SendFriendFile(targetUin, fileStream, fileName);
    
    public static Task SendFriendPoke(this BotContext context, long peerUin, long? targetUin = null)
        => context.EventContext.GetLogic<OperationLogic>().SendPoke(false, peerUin, targetUin ?? context.BotUin);

    public static Task SendGroupPoke(this BotContext context, long peerUin, long targetUin)
        => context.EventContext.GetLogic<OperationLogic>().SendPoke(true, peerUin, targetUin);
}