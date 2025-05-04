using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Entity.Message;
using Lagrange.OneBot.Message;
using Lagrange.OneBot.Utility;

namespace Lagrange.OneBot.Operation.Message;

[Operation("send_group_msg")]
public class SendGroupMessageOperation(MessageService messageService) : IOperation
{
    public Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        throw new NotImplementedException();
    }
}