using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Core.Configuration;
using Lagrange.Milky.Core.Utility;
using Lagrange.Milky.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Lagrange.Core.Events.EventArgs.BotQrCodeQueryEvent;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lagrange.Milky.Core.Service;

public class CoreLoginService(ILogger<CoreLoginService> logger, IOptions<CoreConfiguration> options, IHost host, IHostApplicationLifetime lifetime, BotContext bot, ICaptchaResolver captchaResolver) : IHostedService
{
    private readonly ILogger<CoreLoginService> _logger = logger;
    private readonly CoreConfiguration _configuration = options.Value;
    private readonly IHost _host = host;
    private readonly IHostApplicationLifetime _lifetime = lifetime;
    private readonly BotContext _bot = bot;
    private readonly ICaptchaResolver _captchaResolver = captchaResolver;

    private CancellationTokenSource? _cts;

    public async Task StartAsync(CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        _bot.EventInvoker.RegisterEvent<BotQrCodeEvent>(HandleQrCode);
        _bot.EventInvoker.RegisterEvent<BotQrCodeQueryEvent>(HandleQrCodeQuery);
        _bot.EventInvoker.RegisterEvent<BotRefreshKeystoreEvent>(HandleRefreshKeystore);
        _bot.EventInvoker.RegisterEvent<BotCaptchaEvent>(HandleCaptcha);
        _bot.EventInvoker.RegisterEvent<BotSMSEvent>(HandleSMS);
        _bot.EventInvoker.RegisterEvent<BotNewDeviceVerifyEvent>(HandleNewDeviceVerify);

        uint uin = _configuration.Login.Uin ?? 0;
        string password = _configuration.Login.Password ?? string.Empty;
        bool result = await _bot.Login(uin, password, token);
        if (!result)
        {
            _logger.LogLoginFailed();
            _ = _host.StopAsync(CancellationToken.None);
        }
    }

    private void HandleNewDeviceVerify(BotContext _, BotNewDeviceVerifyEvent @event)
    {
        _logger.LogQrCode(QrCodeUtility.GenerateAscii(@event.Url, _configuration.Login.CompatibleQrCode));
        _logger.LogNewDeviceVerify(_bot.BotUin);
    }

    private async Task HandleSMS(BotContext bot, BotSMSEvent @event)
    {
        // Allow interrupt input
        await Task.Run(() =>
        {
            Console.WriteLine("Please enter the SMS code:");
            string? code = Console.ReadLine();
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogSMSCodeEmpty();
                _host.StopAsync();
                return;
            }

            _bot.SubmitSMSCode(code);
        }, _cts?.Token ?? default);
    }

    private async Task HandleCaptcha(BotContext bot, BotCaptchaEvent @event)
    {
        var (ticket, randstr) = await _captchaResolver.ResolveCaptchaAsync(@event.CaptchaUrl, _cts?.Token ?? default);
        _bot.SubmitCaptcha(ticket, randstr);
    }

    private async Task HandleRefreshKeystore(BotContext bot, BotRefreshKeystoreEvent @event)
    {
        var keystore = @event.Keystore;
        await File.WriteAllBytesAsync(
            $"{keystore.Uin}.keystore",
            CoreJsonUtility.SerializeToUtf8Bytes(keystore),
            _cts?.Token ?? default
        );
    }

    private void HandleQrCodeQuery(BotContext bot, BotQrCodeQueryEvent @event)
    {
        var level = @event.State switch
        {
            TransEmpState.Confirmed or
            TransEmpState.WaitingForScan or
            TransEmpState.WaitingForConfirm => MSLogLevel.Information,
            _ => MSLogLevel.Error,
        };
        _logger.LogQrCodeState(level, @event.State);
    }

    private async Task HandleQrCode(BotContext bot, BotQrCodeEvent @event)
    {
        await File.WriteAllBytesAsync("qrcode.png", @event.Image, _cts?.Token ?? default);

        _logger.LogQrCode(QrCodeUtility.GenerateAscii(@event.Url, _configuration.Login.CompatibleQrCode));
        _logger.LogFetchQrCodeSuccess(@event.Url);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // TODO: unregister
        // _bot.EventInvoker.UnregisterEvent<BotQrCodeEvent>(HandleQrCode);
        // _bot.EventInvoker.UnregisterEvent<BotQrCodeQueryEvent>(HandleQrCodeQuery);
        // _bot.EventInvoker.UnregisterEvent<BotRefreshKeystoreEvent>(HandleRefreshKeystore);
        // _bot.EventInvoker.UnregisterEvent<BotCaptchaEvent>(HandleCaptcha);
        // _bot.EventInvoker.UnregisterEvent<BotSMSEvent>(HandleSMS);
        // _bot.EventInvoker.UnregisterEvent<BotNewDeviceVerifyEvent>(HandleNewDeviceVerify);

        await _bot.Logout();
    }
}

public static partial class CoreLoginServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = MSLogLevel.Information, Message = "\n{qrcode}")]
    public static partial void LogQrCode(this ILogger<CoreLoginService> logger, string qrcode);

    [LoggerMessage(EventId = 1, Level = MSLogLevel.Information, Message = "Fetch QrCode Success, Expiration: 120 seconds, Url: {url}")]
    public static partial void LogFetchQrCodeSuccess(this ILogger<CoreLoginService> logger, string url);

    [LoggerMessage(EventId = 2, Message = "QrCode State: {state}")]
    public static partial void LogQrCodeState(this ILogger<CoreLoginService> logger, MSLogLevel level, TransEmpState state);

    [LoggerMessage(EventId = 3, Level = MSLogLevel.Information, Message = "NewDevice verify required, please scan the QrCode with the device that has already logged in with uin {uin}")]
    public static partial void LogNewDeviceVerify(this ILogger<CoreLoginService> logger, long uin);

    [LoggerMessage(EventId = 998, Level = MSLogLevel.Critical, Message = "Login failed")]
    public static partial void LogLoginFailed(this ILogger<CoreLoginService> logger);

    [LoggerMessage(EventId = 999, Level = MSLogLevel.Critical, Message = "SMS code is empty")]
    public static partial void LogSMSCodeEmpty(this ILogger<CoreLoginService> logger);
}