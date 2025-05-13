using System.Text;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        
        if (!File.Exists("appsettings.json"))
        {
            Console.WriteLine("No exist config file, create it now...");

            await using var istr = typeof(Program).Assembly.GetManifestResourceStream("Lagrange.Milky.Resources.appsettings.json")!;
            await using var temp = File.Create("appsettings.json");
            await istr.CopyToAsync(temp);

            istr.Close();
            temp.Close();

            Console.WriteLine("Please Edit the appsettings.json to set configs and press any key to continue");
            Console.ReadKey(true);
        }
        
        var host = Host.CreateApplicationBuilder(args)
            .Build();

        await host.RunAsync();
    }
}