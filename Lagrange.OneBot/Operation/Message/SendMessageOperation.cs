using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.OneBot.Database;
using Lagrange.OneBot.Entity.Action;

namespace Lagrange.OneBot.Operation.Message;

[Operation("send_msg")]
public class SendMessageOperation(StorageService storage) : IOperation
{
    public Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        throw new NotImplementedException();
    }
}