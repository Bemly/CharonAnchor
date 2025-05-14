using System.Text;
using Lagrange.Milky.Common;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        if (!File.Exists(Constants.ConfigFileName))
        {
            Console.WriteLine("Config file not found, creating a new one...");

            await using var istr = typeof(Program).Assembly
                .GetManifestResourceStream($"Lagrange.Milky.Resources.{Constants.ConfigFileName}")!;
            await using var temp = File.Create(Constants.ConfigFileName);
            await istr.CopyToAsync(temp);

            Console.WriteLine($"Please edit {Constants.ConfigFileName} and press any key to continue.");
            Console.ReadKey(true);
        }
        
        var host = Host.CreateApplicationBuilder(args)
            .UseJsonFileWithComments(Constants.ConfigFileName)
            .Build();

        await host.RunAsync();
    }
}