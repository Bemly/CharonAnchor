using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.OneBot.Entity.Action;

namespace Lagrange.OneBot.Operation;

public interface IOperation
{
    public Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload);
}