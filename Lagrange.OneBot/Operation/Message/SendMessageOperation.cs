using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.OneBot.Database;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Message;
using Lagrange.OneBot.Utility;

namespace Lagrange.OneBot.Operation.Message;

[Operation("send_msg")]
public class SendMessageOperation(MessageService messageService, StorageService storage) : IOperation
{
    public async Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        if (payload.Deserialize<OneBotMessage>() is not { } message) throw new Exception();

        var chain = messageService.ConvertToChain(message);

        if (message is { MessageType: "group", GroupId: { } groupId })
        {
            var result = await context.SendGroupMessage(chain, groupId);
            int hash = StorageService.CalcMessageHash(result.MessageId, result.Sequence);
            return new OneBotResult(new OneBotMessageResponse(hash), 0, "ok");
        }
        
        if (message is { MessageType: "private", UserId: { } userId })
        {
            var result = await context.SendFriendMessage(chain, userId);
            await storage.SaveMessage(result, messageService.ConvertSendMsgToPush(result).ToArray());
        
            int hash = StorageService.CalcMessageHash(result.MessageId, result.Sequence);
            return new OneBotResult(new OneBotMessageResponse(hash), 0, "ok");
        }
        
        throw new Exception();
    }
}