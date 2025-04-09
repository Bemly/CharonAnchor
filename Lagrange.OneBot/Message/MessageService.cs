using Microsoft.Extensions.Configuration;

namespace Lagrange.OneBot.Message;

public class MessageService
{
    private readonly MessageOption _option = new();
    
    public MessageService(IConfiguration config)
    {
        config.GetSection("Message").Bind(_option);
    }
}