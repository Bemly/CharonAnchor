using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky.Extension;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureConfiguration(this HostApplicationBuilder builder, Action<ConfigurationManager> configurer)
    {
        configurer(builder.Configuration);
        return builder;
    }

    public static HostApplicationBuilder ConfigureServices(this HostApplicationBuilder builder, Action<IServiceCollection> configurer)
    {
        configurer(builder.Services);
        return builder;
    }
}