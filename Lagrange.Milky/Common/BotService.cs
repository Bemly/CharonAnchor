using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Common.Config;
using Lagrange.Milky.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lagrange.Milky.Common;

public partial class BotService(
    ILoggerFactory loggerFactory,
    BotContext context,
    IConfiguration config,
    IOptionsSnapshot<AccountConfig> options,
    ICaptchaResolver captchaResolver) : IHostedService
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<BotService>();

    private readonly ConcurrentDictionary<string, ILogger> _contextLoggers = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        context.EventInvoker.RegisterEvent<BotLogEvent>((_, @event) =>
        {
            var level = @event.Level switch
            {
                Lagrange.Core.Events.EventArgs.LogLevel.Critical => LogLevel.Critical,
                Lagrange.Core.Events.EventArgs.LogLevel.Error => LogLevel.Error,
                Lagrange.Core.Events.EventArgs.LogLevel.Warning => LogLevel.Warning,
                Lagrange.Core.Events.EventArgs.LogLevel.Information => LogLevel.Information,
                Lagrange.Core.Events.EventArgs.LogLevel.Debug => LogLevel.Debug,
                Lagrange.Core.Events.EventArgs.LogLevel.Trace => LogLevel.Trace,
                _ => throw new ArgumentOutOfRangeException()
            };

            var contextLogger = _contextLoggers.GetOrAdd(@event.Tag, _ => loggerFactory.CreateLogger(InferFullName(@event.Tag)));
            Log.LogBotMessage(contextLogger, level, @event.Message);
        });

        context.EventInvoker.RegisterEvent<BotQrCodeEvent>(async (_, @event) =>
        {
            await File.WriteAllBytesAsync("qrcode.png", @event.Image, cancellationToken);
            bool compatibilityMode = config.GetValue<bool>("Login:QrCodeConsoleCompatibilityMode");
            QrCodeHelper.Output(@event.Url, compatibilityMode);
            Log.QrCodeSuccess(_logger, 120, @event.Url);
        });

        context.EventInvoker.RegisterEvent<BotQrCodeQueryEvent>((_, @event) =>
        {
            var level = @event.State switch
            {
                BotQrCodeQueryEvent.TransEmpState.Confirmed or BotQrCodeQueryEvent.TransEmpState.WaitingForConfirm => LogLevel.Information,
                BotQrCodeQueryEvent.TransEmpState.Canceled or BotQrCodeQueryEvent.TransEmpState.CodeExpired => LogLevel.Error,
                _ => LogLevel.Debug
            };
            Log.QrCodeState(_logger, level, @event.State);
        });

        context.EventInvoker.RegisterEvent<BotRefreshKeystoreEvent>(async (_, @event) =>
        {
            var keystore = @event.Keystore;
            await File.WriteAllBytesAsync($"Lagrange-{keystore.Uin}.keystore", JsonHelper.SerializeToUtf8Bytes(keystore), cancellationToken);
        });

        context.EventInvoker.RegisterEvent<BotCaptchaEvent>(async (_, @event) =>
        {
            var (ticket, randstr) = await captchaResolver.ResolveCaptchaAsync(@event.CaptchaUrl, cancellationToken);
            context.SubmitCaptcha(ticket, randstr);
        });

        context.EventInvoker.RegisterEvent<BotSMSEvent>(async (_, _) =>
        {
            await Task.Run(() =>
            {
                Console.WriteLine("Please enter the SMS code:");
                string? code = Console.ReadLine();
                if (string.IsNullOrEmpty(code))
                {
                    _logger.LogCritical("SMS code is empty, process would exit in 10 seconds");
                    Environment.Exit(-1);
                }

                context.SubmitSMSCode(code);
            }, cancellationToken);
        });

        context.EventInvoker.RegisterEvent<BotNewDeviceVerifyEvent>((_, @event) =>
        {
            Log.NewDeviceVerify(_logger, context.BotUin);
            bool compatibilityMode = config.GetValue<bool>("Login:QrCodeConsoleCompatibilityMode");
            QrCodeHelper.Output(@event.Url, compatibilityMode);
        });

        bool result = await context.Login(options.Value.Uin, options.Value.Password ?? string.Empty, cancellationToken);
        if (!result)
        {
            _logger.LogCritical("Login failed, process would exit in 10 seconds");
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            Environment.Exit(-1);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await context.Logout();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "It does not matter")]
    private static string InferFullName(string tag)
    {
        foreach (var type in typeof(BotContext).Assembly.GetTypes())
        {
            if (type.Name == tag)
            {
                return type.FullName ?? type.Name;
            }
        }

        return tag;
    }

    private static partial class Log
    {
        [LoggerMessage(Message = "{message}", EventId = 0)]
        public static partial void LogBotMessage(ILogger logger, LogLevel level, string message);

        [LoggerMessage(Level = LogLevel.Information, EventId = 0, Message = "Fetch QrCode Success, Expiration: {expiration} seconds, Url: {url}")]
        public static partial void QrCodeSuccess(ILogger logger, int expiration, string url);

        [LoggerMessage(EventId = 1, Message = "QrCode State: {state}")]
        public static partial void QrCodeState(ILogger logger, LogLevel level, BotQrCodeQueryEvent.TransEmpState state);

        [LoggerMessage(Level = LogLevel.Information, EventId = 3, Message = "NewDevice verify required, please scan the QrCode with the device that has already logged in with uin {uin}")]
        public static partial void NewDeviceVerify(ILogger logger, long uin);
    }
}