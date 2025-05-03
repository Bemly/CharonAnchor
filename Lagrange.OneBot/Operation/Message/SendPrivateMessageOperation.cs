using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.OneBot.Database;
using Lagrange.OneBot.Entity.Action;

namespace Lagrange.OneBot.Operation.Message;

[Operation("send_private_msg")]
public class SendPrivateMessageOperation(StorageService storage) : IOperation
{
    public Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        throw new NotImplementedException();
    }
}