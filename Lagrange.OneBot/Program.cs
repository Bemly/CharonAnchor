using Lagrange.OneBot.Services;
using Lagrange.OneBot.Utility.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.OneBot;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateApplicationBuilder(args)
            .ConfigureCore()
            .ConfigureSQLite()
            .ConfigureServices()
            .Build();

        await host.RunAsync();
    }
}