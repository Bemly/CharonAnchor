using System.Text;
using Lagrange.Milky.Core.Extension;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        ShowApplicationInfo();

        CheckConfigurationFile();

        await Host.CreateApplicationBuilder(args)
            .ConfigureConfiguration(configuration => configuration
                .AddJsonFile(Path.GetFullPath(Constants.ConfigFileName), false, true)
            )
            .ConfigureCore()
            .ConfigureMilky()
            .ConfigureCoreLogin()
            .Build()
            .RunAsync();
    }

    private static void CheckConfigurationFile()
    {
        if (!File.Exists(Constants.ConfigFileName))
        {
            {
                Console.WriteLine($"{Constants.ConfigFileName} not found. Generating...");
                using var input = typeof(Program).Assembly.GetManifestResourceStream(Constants.ConfigResourceName);
                if (input == null) throw new Exception("Default configuration file not found");
                using var output = File.OpenWrite(Constants.ConfigFileName);
                input.CopyTo(output);
            }

            Console.WriteLine("Please edit the configuration file");
            Console.WriteLine("and press any key to continue starting the application.");
            Console.ReadKey();
        }
    }

    private static void ShowApplicationInfo()
    {
        Console.WriteLine(Constants.Banner);

        Console.WriteLine($"Version: {Constants.ImplementationVersion}");
    }
}