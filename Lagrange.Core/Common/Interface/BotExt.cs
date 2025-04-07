using Lagrange.Core.Internal.Logic;

namespace Lagrange.Core.Common.Interface;

public static class BotExt
{
    public static Task<bool> Login(this BotContext context, long uin, string password) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Login(uin, password);
    
    public static Task<bool> Login(this BotContext context) => 
        context.EventContext.GetLogic<WtExchangeLogic>().Login(0, null);
}