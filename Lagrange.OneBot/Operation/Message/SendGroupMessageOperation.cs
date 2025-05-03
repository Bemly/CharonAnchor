using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.OneBot.Entity.Action;

namespace Lagrange.OneBot.Operation.Message;

[Operation("send_group_msg")]
public class SendGroupMessageOperation : IOperation
{
    public Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        throw new NotImplementedException();
    }
}