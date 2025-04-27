using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Utility;

namespace Lagrange.OneBot.Operation.Generic;

[Operation("close_qrcode")]
public class CloseQrCodeOperation : IOperation
{
    public async Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        if (payload.Deserialize<OneBotQrCodeRequest>() is not { } request) throw new Exception();

        string k = request.K ?? request.Url.Split('?')[1].Split('&').ToDictionary(
            x => x.Split('=')[0],
            x => x.Split('=')[1]
        )["k"];
        
        var kCode = Convert.FromBase64String(k.Replace('*', '+').Replace('-', '/').Replace("==", ""));
        var (result, message) = await context.CloseQrCode(kCode, request.Confirm);
        var json = new JsonObject
        {
            ["success"] = result,
            ["message"] = message
        };
        
        return new OneBotResult(json, 0, "ok");
    }
}