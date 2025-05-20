using System.Net;
using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.Milky.Core.Configuration;
using Lagrange.Milky.Core.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Utility;

public partial class OnlineCaptchaResolver(ILogger<OnlineCaptchaResolver> logger, IOptions<CoreConfiguration> options, BotContext bot) : ICaptchaResolver
{
    private const string Url = "https://captcha.lagrangecore.org/?{0}";
    private const string QueryUrl = "https://backend.captcha.lagrangecore.org/get_captcha?uin={0}";

    private readonly ILogger<OnlineCaptchaResolver> _logger = logger;
    private readonly CoreConfiguration _configuration = options.Value;
    private readonly BotContext _bot = bot;

    private readonly HttpClient _client = new();

    public async Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token)
    {
        string solveUrl = string.Format(Url, url.Split('?')[1].Replace("uin=0", $"uin={_bot.BotUin}"));
        _logger.LogCaptchaQrCode(QrCodeUtility.GenerateAscii(solveUrl, _configuration.Login.CompatibleQrCode));
        _logger.LogCaptchaTip(solveUrl);

        while (true)
        {
            token.ThrowIfCancellationRequested();

            string queryUrl = string.Format(QueryUrl, _bot.BotUin);
            var response = await _client.GetAsync(queryUrl, token);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogCaptchaWaiting();
                continue;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected http status code({response.StatusCode})");
            }

            string result = await response.Content.ReadAsStringAsync(token);
            string? json = JsonNode.Parse(result)?["data"]?.GetValue<string>();
            if (json == null) continue;

            return (json.Split('|')[0], json.Split('|')[1]);
        }
    }
}

public static partial class OnlineCaptchaResolverLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "\n{qrcode}")]
    public static partial void LogCaptchaQrCode(this ILogger<OnlineCaptchaResolver> logger, string qrcode);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Please scan the QR code or access the {url} to solve the captcha")]
    public static partial void LogCaptchaTip(this ILogger<OnlineCaptchaResolver> logger, string url);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Waiting for captcha response...")]
    public static partial void LogCaptchaWaiting(this ILogger<OnlineCaptchaResolver> logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Captcha solved, ticket: {ticket}, randstr: {randstr}")]
    public static partial void CaptchaSolved(this ILogger<OnlineCaptchaResolver> logger, string ticket, string randstr);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "Unexpected http status code({status})")]
    public static partial void LogCaptchaError(this ILogger<OnlineCaptchaResolver> logger, HttpStatusCode status);
}