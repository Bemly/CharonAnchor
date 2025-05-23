using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Service;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky.Implementation.Extension;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureMilky(this HostApplicationBuilder builder) => builder
        .ConfigureServices(services => services
            .Configure<MilkyConfiguration>(builder.Configuration.GetSection("Milky"))

            // EntityConvert
            .AddSingleton<EntityConvert>()

            // Api
            .AddApiHandlers()

            // Event
            .AddSingleton<EventConvert>()
            .AddSingleton<EventService>()
            .AddHostedService(ServiceProviderServiceExtensions.GetRequiredService<EventService>)
        )
        .ConfigureServices(services =>
        {
            // WebSocket
            var configuration = builder.Configuration.GetSection("Milky").Get<MilkyConfiguration>();
            if (configuration?.WebSocket != null)
            {
                services.AddHostedService<WebSocketService>();
            }
        });
}