using Lagrange.Core.Common.Response;
using Lagrange.Core.Internal.Logic;

namespace Lagrange.Core.Common.Interface;

public static class OperationExt
{
    public static Task<BotQrCodeInfo?> FetchQrCodeInfo(this BotContext context, byte[] k) => 
        context.EventContext.GetLogic<WtExchangeLogic>().FetchQrCodeInfo(k);
    
    public static Task<(bool Success, string Message)> CloseQrCode(this BotContext context, byte[] k, bool confirm) =>
        context.EventContext.GetLogic<WtExchangeLogic>().CloseQrCode(k, confirm);
}