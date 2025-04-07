using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.Runner;

internal static class Program
{
    private static async Task Main()
    {
        var context = BotFactory.Create(new BotConfig
        {
            
        });
        
        context.EventInvoker.RegisterEvent<BotLogEvent>((_, args) =>
        {
            Console.WriteLine(args);
        });

        await context.Login();
        await Task.Delay(-1);
    }
}