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

                var platform = coreConfiguration.Protocol.Platform;
                if (!platform.HasValue) throw new Exception("Core.Protocol.Platform cannot be null");

                // Perform simple verification
                // Since AndroidPhone and AndroidPad cannot be recognized
                // So the platform provided by signer is not used directly
                // TODO /appinfo_v2
                // var verified = signer.GetAppInfo().Result.Os switch
                // {
                //     "Linux" => platform == Protocols.Linux,
                //     "Mac" => platform == Protocols.MacOs,
                //     "Windows" => platform == Protocols.Windows,
                //     "Android" => platform == Protocols.AndroidPhone,
                //     _ => false,
                // };
                // if (verified) throw new Exception(
                //     "The protocol used to generate the signature does not match the configured protocol."
                // );

                return new BotConfig
                {
                    Protocol = platform.Value,
                    LogLevel = (CoreLogLevel)loggerConfiguration.GetDefaultLogLevel(),
                    AutoReconnect = coreConfiguration.Server.AutoReconnect,
                    UseIPv6Network = coreConfiguration.Server.UseIPv6Network,
                    GetOptimumServer = coreConfiguration.Server.GetOptimumServer,
                    AutoReLogin = coreConfiguration.Login.AutoReLogin,
                    SignProvider = signer,
                };
            })
            // BotKeystore
            // .AddSingleton(services =>
            // {
            //     var configuration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;

            //     if (!configuration.Login.Uin.HasValue) throw new Exception("Core.Login.Uin cannot be null");
            //     var path = $"{configuration.Login.Uin.Value}.keystore";

            //     BotKeystore keystore;
            //     if (File.Exists(path))
            //     {
            //         var keystoreNullable = CoreJsonUtility.Deserialize<BotKeystore>(File.ReadAllBytes(path));
            //         keystore = keystoreNullable ?? throw new Exception(
            //             $"Invalid keystore detected. Please remove the '{path}' file and re-authenticate."
            //         );
            //     }
            //     else
            //     {
            //         keystore = BotKeystore.CreateEmpty();
            //     }

            //     keystore.DeviceName = configuration.Login.DeviceName;
            //     return keystore;
            // })
            // BotAppInfo
            // TODO /appinfo_v2
            .AddSingleton(services => services.GetRequiredService<Signer>().GetAppInfo().Result)
            .AddSingleton(services =>
            {
                var configuration = services.GetRequiredService<IOptions<CoreConfiguration>>().Value;

                var platform = configuration.Protocol.Platform;
                if (!platform.HasValue) throw new Exception("Core.Protocol.Platform cannot be null");

                return BotAppInfo.ProtocolToAppInfo[platform.Value];
            })
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