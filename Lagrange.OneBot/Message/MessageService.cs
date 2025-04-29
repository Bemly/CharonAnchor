using Lagrange.Core;
using Lagrange.Core.Events.EventArgs;
using Lagrange.OneBot.Database;
using Microsoft.Extensions.Configuration;

namespace Lagrange.OneBot.Message;

public class MessageService
{
    private readonly MessageOption _option = new();
    
    public MessageService(IConfiguration config, BotContext context, StorageService storage)
    {
        config.GetSection("Message").Bind(_option);
        
        context.EventInvoker.RegisterEvent<BotMessageEvent>(async (_, @event) =>
        {
            await storage.SaveMessage(@event);
        });
    }
}