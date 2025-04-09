using System.Reflection;
using Lagrange.OneBot.Services;
using Lagrange.OneBot.Utility.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.OneBot;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        if (!File.Exists("appsettings.json"))
        {
            Console.WriteLine("No exist config file, create it now...");

            var assm = Assembly.GetExecutingAssembly();
            await using var istr = assm.GetManifestResourceStream("Lagrange.OneBot.Resources.appsettings.json")!;
            await using var temp = File.Create("appsettings.json");
            await istr.CopyToAsync(temp);

            istr.Close();
            temp.Close();

            Console.WriteLine("Please Edit the appsettings.json to set configs and press any key to continue");
            Console.ReadKey(true);
        }
        
        var host = Host.CreateApplicationBuilder(args)
            .ConfigureCore()
            .ConfigureSQLite()
            .ConfigureServices()
            .ConfigureOneBot()
            .Build();

        await host.RunAsync();
    }
}