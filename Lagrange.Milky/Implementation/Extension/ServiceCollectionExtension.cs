using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lagrange.Milky.Implementation.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Lagrange.Milky.Implementation.Extension;

public static class ServiceCollectionExtension
{
    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    [UnconditionalSuppressMessage("Trimming", "IL2072")]
    public static TServiceCollection AddApiHandlers<TServiceCollection>(this TServiceCollection services) where TServiceCollection : IServiceCollection
    {
        foreach (var type in typeof(ServiceCollectionExtension).Assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<ApiAttribute>();
            if (attribute == null) continue;

            if (!type.IsAssignableTo(typeof(IApiHandler)))
            {
                throw new Exception("Api handler must implement the IApiHandler");
            }

            services.AddKeyedSingleton(typeof(IApiHandler), attribute.Name, type);
        }

        return services;
    }
}