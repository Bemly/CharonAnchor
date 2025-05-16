using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lagrange.Milky.Implementation.Apis;
using Lagrange.Milky.Implementation.Configurations;
using Lagrange.Milky.Implementation.Events;
using Lagrange.Milky.Implementation.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky.Implementation.Extensions;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder AddMilky(this HostApplicationBuilder builder)
    {
        builder.Services.Configure<MilkyConfiguration>(builder.Configuration.GetSection("Milky"));

        builder.AddMilkyApiHandler();

        builder.Services.AddSingleton<MilkyApiHandler>();
        builder.Services.AddSingleton<MilkyEventHandler>();
        builder.Services.AddHostedService<MilkyService>();

        return builder;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    private static HostApplicationBuilder AddMilkyApiHandler(this HostApplicationBuilder builder)
    {
        var types = typeof(HostApplicationBuilderExtension).Assembly.GetTypes();
        foreach (var type in types)
        {
            foreach (var attribute in type.GetCustomAttributes<ApiAttribute>())
            {
                if (!type.IsAssignableTo(typeof(IApiHandler)))
                {
                    throw new Exception($"Classes({type}) using ApiAttribute must implement IApiHandler");
                }

                builder.Services.AddKeyedSingleton(typeof(IApiHandler), attribute.Api, type);
            }
        }

        return builder;
    }
}