using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.OneBot.Entity.Action;

namespace Lagrange.OneBot.Operation.Ability;

[Operation("can_send_image")]
public class CanSendImage : IOperation
{
    public Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload) => 
        Task.FromResult(new OneBotResult(new JsonObject { { "yes", Constant.CanSendImage } }, 0, "ok"));
}