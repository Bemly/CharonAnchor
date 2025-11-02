using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.NativeAPI.NativeModel.Common;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Runner;

internal static class Program
{
    private static async Task Main()
    {
        var sign = new LinuxSignProvider("");
        
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        
        BotContext context;

        if (File.Exists("keystore.json"))
        {
            context = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.Linux,
                SignProvider = sign,
                LogLevel = LogLevel.Debug
            }, JsonSerializer.Deserialize<BotKeystore>(await File.ReadAllTextAsync("keystore.json")) ?? throw new InvalidOperationException());
        }
        else
        {
            context = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.Linux,
                SignProvider = sign,
                LogLevel = LogLevel.Debug
            });
        }
        
        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            await context.Logout();
        };
        
        context.EventInvoker.RegisterEvent<BotLogEvent>((_, args) =>
        {
            Console.WriteLine(args);
        });
        
        context.EventInvoker.RegisterEvent<BotQrCodeEvent>((_, args) =>
        {
            Console.WriteLine(args);
            QrCodeHelper.Output(args.Url, false);
        });
        
        context.EventInvoker.RegisterEvent<BotRefreshKeystoreEvent>(async (_, args) =>
        {
            await File.WriteAllTextAsync("keystore.json", JsonSerializer.Serialize(args.Keystore));
        });

        await context.Login();

        //var builder = new MessageBuilder().Text("Awoo");
        //var message = await context.SendFriendMessage(1925648680, builder.Build());
        
        await Task.Delay(-1);
    }
}