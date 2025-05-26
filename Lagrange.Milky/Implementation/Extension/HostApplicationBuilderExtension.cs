using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Communication;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Lagrange.Milky.Implementation.Event;

namespace Lagrange.Milky.Implementation.Extension;

public static partial class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureMilky(this HostApplicationBuilder builder) => builder
        .ConfigureServices(services => services
            .Configure<MilkyConfiguration>(builder.Configuration.GetSection("Milky"))
            // Converter
            .AddSingleton<Converter>()
            // Api Handlers
            .AddApiHandlers()
        )
        .ConfigureServices(services =>
        {
            services.AddHostedService<MilkyHttpApiService>();

            var configuration = builder.Configuration.GetSection("Milky").Get<MilkyConfiguration>();

            if (configuration?.EventPath != null || configuration?.WebHook != null)
            {
                services.AddSingleton<EventService>();
                services.AddHostedService(ServiceProviderServiceExtensions.GetRequiredService<EventService>);
            }

            if (configuration?.EventPath != null) services.AddHostedService<MilkyWebSocketEventService>();
        });
}