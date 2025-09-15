using Microsoft.Extensions.DependencyInjection;

namespace Lagrange.Milky.Extension;

public static partial class ServiceCollectionExtension
{
    public static TServiceCollection AddApiHandlers<TServiceCollection>(this TServiceCollection services, bool debug) where TServiceCollection : IServiceCollection
    {
        AddApiHandlersInternal(services, debug);
        return services;
    }

    static partial void AddApiHandlersInternal<TServiceCollection>(TServiceCollection services, bool debug) where TServiceCollection : IServiceCollection;
}