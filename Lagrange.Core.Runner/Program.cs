using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Runner.Utility;

namespace Lagrange.Core.Runner;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        BotContext? bot = null;

        var sign = new Signer(new Lazy<BotContext>(() => bot!), "http://106.54.14.24:8084/api/sign/30366");

        if (File.Exists("keystore.json"))
        {
            bot = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.Linux,
                SignProvider = sign,
                LogLevel = LogLevel.Trace,
            }, JsonSerializer.Deserialize<BotKeystore>(await File.ReadAllTextAsync("keystore.json")) ?? throw new InvalidOperationException());
        }
        else
        {
            bot = BotFactory.Create(new BotConfig
            {
                Protocol = Protocols.Linux,
                SignProvider = sign
            });
        }

        AppDomain.CurrentDomain.ProcessExit += async (_, _) =>
        {
            await bot.Logout();
        };

        bot.EventInvoker.RegisterEvent<BotLogEvent>((_, args) =>
        {
            Console.WriteLine(args);
        });

        bot.EventInvoker.RegisterEvent<BotQrCodeEvent>((_, args) =>
        {
            Console.WriteLine(args);
            Console.WriteLine(QrCodeUtility.GenerateAscii(args.Url, false));
        });

        bot.EventInvoker.RegisterEvent<BotRefreshKeystoreEvent>(async (_, args) =>
        {
            await File.WriteAllTextAsync("keystore.json", JsonSerializer.Serialize(args.Keystore));
        });

        await bot.Login();
        await Task.Delay(5000);

        using (var stream = File.OpenRead(@"E:\Code\CSharp\Lagrange\LagrangeV2\Lagrange.Core.Runner\Program.cs"))
        {
            if (!await bot.SendGroupFile(907925148, stream, "Program.cs", "/"))
            {
                Console.WriteLine("Send group file failed");
            }
            else Console.WriteLine("Send group file succeed");

            if (!await bot.SendFriendFile(1224702603, stream, "Program.cs"))
            {
                Console.WriteLine("Send friend file failed");
            }
            else Console.WriteLine("Send friend file succeed");
        }

        await Task.Delay(-1);
    }
}