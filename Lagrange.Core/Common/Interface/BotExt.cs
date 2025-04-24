using Lagrange.Core.Internal.Logic;

namespace Lagrange.Core.Common.Interface;

public static class BotExt
{
    public static Task<bool> Login(this BotContext context, long uin, string password, CancellationToken token = default) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Login(uin, password, token);
    
    public static Task<bool> Login(this BotContext context, CancellationToken token = default) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Login(0, null, token);

    public static bool SubmitCaptcha(this BotContext context, string ticket, string randStr) =>
        context.EventContext.GetLogic<WtExchangeLogic>().SubmitCaptcha(ticket, randStr);
}