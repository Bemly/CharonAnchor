using Lagrange.Core.Internal.Logic;
using Lagrange.Core.Message;

namespace Lagrange.Core.Common.Interface;

public static class MessageExt
{
    public static Task<BotMessage> SendFriendMessage(this BotContext context, MessageChain chain, long friendUin) 
        => context.EventContext.GetLogic<MessagingLogic>().SendFriendMessage(chain, friendUin);

    public static Task<BotMessage> SendGroupMessage(this BotContext context, MessageChain chain, long groupUin)
        => context.EventContext.GetLogic<MessagingLogic>().SendGroupMessage(chain, groupUin);
}