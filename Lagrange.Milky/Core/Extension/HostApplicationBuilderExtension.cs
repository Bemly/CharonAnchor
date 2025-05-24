using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Core.Configuration;
using Lagrange.Milky.Core.Service;
using Lagrange.Milky.Core.Utility;
using Lagrange.Milky.Core.Utility.CaptchaResolver;
using Lagrange.Milky.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreLogLevel = Lagrange.Core.Events.EventArgs.LogLevel;


namespace Lagrange.Milky.Core.Extension;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureCore(this HostApplicationBuilder builder) => builder
        .ConfigureServices(services => services
            .Configure<CoreConfiguration>(builder.Configuration.GetSection("Core"))

            .AddSingleton(services => ActivatorUtilities.CreateInstance<Signer>(services,
                new Lazy<BotContext>(services.GetRequiredService<BotContext>)
            ))
            // BotConfig
            .AddSingleton(services =>
            {
                var loggerConfiguration = services.GetRequiredService<IOptions<LoggerFilterOptions>>().Value;
                var coreConfiguration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;
                var signer = services.GetRequiredService<Signer>();

                var platform = signer.GetAppInfo().Result.Os switch
                {
                    "Linux" => Protocols.Linux,
                    "Mac" => Protocols.MacOs,
                    "Windows" => Protocols.Windows,
                    "Android" => Protocols.AndroidPhone,
                    _ => throw new NotSupportedException(),
                };

                return new BotConfig
                {
                    Protocol = platform,
                    LogLevel = (CoreLogLevel)loggerConfiguration.GetDefaultLogLevel(),
                    AutoReconnect = coreConfiguration.Server.AutoReconnect,
                    UseIPv6Network = coreConfiguration.Server.UseIPv6Network,
                    GetOptimumServer = coreConfiguration.Server.GetOptimumServer,
                    AutoReLogin = coreConfiguration.Login.AutoReLogin,
                    SignProvider = signer,
                };
            })
            // BotKeystore
            .AddSingleton(services =>
            {
                var configuration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;

                if (!configuration.Login.Uin.HasValue) throw new Exception("Core.Login.Uin cannot be null");
                var path = $"{configuration.Login.Uin.Value}.keystore";

                BotKeystore keystore;
                if (File.Exists(path))
                {
                    var keystoreNullable = CoreJsonUtility.Deserialize<BotKeystore>(File.ReadAllBytes(path));
                    keystore = keystoreNullable ?? throw new Exception(
                        $"Invalid keystore detected. Please remove the '{path}' file and re-authenticate."
                    );
                }
                else
                {
                    keystore = BotKeystore.CreateEmpty();
                }

                keystore.DeviceName = configuration.Login.DeviceName;
                return keystore;
            })
            // BotAppInfo
            .AddSingleton(services => services.GetRequiredService<Signer>().GetAppInfo().Result)
            // BotContext
            .AddSingleton(services =>
            {
                var config = services.GetRequiredService<BotConfig>();
                var keystore = services.GetRequiredService<BotKeystore>();
                var info = services.GetRequiredService<BotAppInfo>();

                return BotFactory.Create(config, keystore, info);
            })

            // CaptchaResolver
            .AddSingleton<ICaptchaResolver>(services =>
            {
                var configuration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;

                return configuration.Login.UseOnlineCaptchaResolver
                    ? ActivatorUtilities.CreateInstance<OnlineCaptchaResolver>(services)
                    : ActivatorUtilities.CreateInstance<ManualCaptchaResolver>(services);
            })

            // CoreLoggerService
            .AddHostedService<CoreLoggerService>()
        );

    public static HostApplicationBuilder ConfigureCoreLogin(this HostApplicationBuilder builder) => builder
        .ConfigureServices(services => services
            .AddHostedService<CoreLoginService>()
        );
}