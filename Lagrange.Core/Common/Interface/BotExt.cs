using Lagrange.Core.Internal.Logic;

namespace Lagrange.Core.Common.Interface;

public static class BotExt
{
    public static Task<bool> Login(this BotContext context, long uin, string password, CancellationToken token = default) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Login(uin, password, token);
    
    public static Task<bool> Login(this BotContext context, CancellationToken token = default) =>
        context.EventContext.GetLogic<WtExchangeLogic>().Login(0, null, token);
    
    public static Task<long> ResolveUinByQid(this BotContext context, string qid) =>
        context.EventContext.GetLogic<WtExchangeLogic>().ResolveUinByQid(qid);

    public static bool SubmitCaptcha(this BotContext context, string ticket, string randStr) =>
        context.EventContext.GetLogic<WtExchangeLogic>().SubmitCaptcha(ticket, randStr);
}