using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Core.Configurations;
using Lagrange.Milky.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;


namespace Lagrange.Milky.Core.Services;

public partial class LagrnageLoginService(IHost host, ILogger<LagrnageLoginService> logger, BotContext bot, IOptions<LagrangeConfiguration> options, ICaptchaResolver captchaResolver) : IHostedService
{
    private readonly IHost _host = host;
    private readonly ILogger<LagrnageLoginService> _logger = logger;
    private readonly BotContext _bot = bot;
    private readonly LagrangeConfiguration _config = options.Value;
    private readonly ICaptchaResolver _captchaResolver = captchaResolver;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _bot.EventInvoker.RegisterEvent<BotQrCodeEvent>(async (_, @event) =>
        {
            await File.WriteAllBytesAsync("qrcode.png", @event.Image, cancellationToken);
            bool compatibilityMode = _config.Login.QrCodeConsoleCompatibilityMode;
            QrCodeHelper.Output(@event.Url, compatibilityMode);
            _logger.QrCodeSuccess(120, @event.Url);
        });

        _bot.EventInvoker.RegisterEvent<BotQrCodeQueryEvent>((_, @event) =>
        {
            var level = @event.State switch
            {
                BotQrCodeQueryEvent.TransEmpState.Confirmed or
                BotQrCodeQueryEvent.TransEmpState.WaitingForConfirm => MSLogLevel.Information,
                BotQrCodeQueryEvent.TransEmpState.Canceled or
                BotQrCodeQueryEvent.TransEmpState.CodeExpired => MSLogLevel.Error,
                _ => MSLogLevel.Debug
            };
            _logger.QrCodeState(level, @event.State);
        });

        _bot.EventInvoker.RegisterEvent<BotRefreshKeystoreEvent>(async (_, @event) =>
        {
            var keystore = @event.Keystore;
            await File.WriteAllBytesAsync($"{keystore.Uin}.keystore", JsonHelper.SerializeToUtf8Bytes(keystore), cancellationToken);
        });

        _bot.EventInvoker.RegisterEvent<BotCaptchaEvent>(async (_, @event) =>
        {
            var (ticket, randstr) = await _captchaResolver.ResolveCaptchaAsync(@event.CaptchaUrl, cancellationToken);
            _bot.SubmitCaptcha(ticket, randstr);
        });

        _bot.EventInvoker.RegisterEvent<BotSMSEvent>(async (_, _) =>
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

                _bot.SubmitSMSCode(code);
            }, cancellationToken);
        });

        _bot.EventInvoker.RegisterEvent<BotNewDeviceVerifyEvent>((_, @event) =>
        {
            _logger.NewDeviceVerify(_bot.BotUin);
            bool compatibilityMode = _config.Login.QrCodeConsoleCompatibilityMode;
            QrCodeHelper.Output(@event.Url, compatibilityMode);
        });

        bool result = await _bot.Login((long)_config.Login.Uin, _config.Login.Password ?? string.Empty, cancellationToken);
        if (!result)
        {
            _logger.LoginFailed();
            _ = _host.StopAsync(default);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _bot.Logout();
    }
}

public static partial class CoreLoginServiceLoggerExtension
{
    [LoggerMessage(Level = MSLogLevel.Information, EventId = 0, Message = "Fetch QrCode Success, Expiration: {expiration} seconds, Url: {url}")]
    public static partial void QrCodeSuccess(this ILogger<LagrnageLoginService> logger, int expiration, string url);

    [LoggerMessage(EventId = 1, Message = "QrCode State: {state}")]
    public static partial void QrCodeState(this ILogger<LagrnageLoginService> logger, MSLogLevel level, BotQrCodeQueryEvent.TransEmpState state);

    [LoggerMessage(Level = MSLogLevel.Information, EventId = 2, Message = "NewDevice verify required, please scan the QrCode with the device that has already logged in with uin {uin}")]
    public static partial void NewDeviceVerify(this ILogger<LagrnageLoginService> logger, long uin);

    [LoggerMessage(Level = MSLogLevel.Critical, EventId = 3, Message = "Login failed, process would exit in 10 seconds")]
    public static partial void LoginFailed(this ILogger<LagrnageLoginService> logger);

}