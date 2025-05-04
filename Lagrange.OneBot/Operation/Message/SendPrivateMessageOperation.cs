using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.OneBot.Database;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Entity.Message;
using Lagrange.OneBot.Message;
using Lagrange.OneBot.Utility;

namespace Lagrange.OneBot.Operation.Message;

[Operation("send_private_msg")]
public class SendPrivateMessageOperation(StorageService storage, MessageService messageService) : IOperation
{
    public async Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        if (payload.Deserialize<OneBotMessage>() is not { UserId: { } userId } message) throw new Exception();

        var chain = messageService.ConvertToChain(message);
        var result = await context.SendFriendMessage(chain, userId);
        var raw = messageService.ConvertSendMsgToPush(result);
        await storage.SaveMessage(result, raw.ToArray());
        
        int hash = StorageService.CalcMessageHash(result.MessageId, result.Sequence);
        return new OneBotResult(new OneBotMessageResponse(hash), 0, "ok");
    }
}