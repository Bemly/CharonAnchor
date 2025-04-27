using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Utility;

namespace Lagrange.OneBot.Operation.Generic;

[Operation("fetch_qrcode_info")]
public class FetchQrCodeInfoOperation : IOperation
{
    public async Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        if (payload.Deserialize<OneBotQrCodeRequest>() is not { } request) throw new Exception();

        string k = request.K ?? request.Url.Split('?')[1].Split('&').ToDictionary(
            x => x.Split('=')[0],
            x => x.Split('=')[1]
        )["k"];
        
        var kCode = Convert.FromBase64String(k.Replace('*', '+').Replace('-', '/').Replace("==", ""));
        var result = await context.FetchQrCodeInfo(kCode);

        return new OneBotResult(result, 0, "ok");
    }
}