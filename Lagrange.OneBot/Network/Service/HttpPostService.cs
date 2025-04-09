using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lagrange.Core;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Entity.Meta;
using Lagrange.OneBot.Network.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.OneBot.Network.Service;

public partial class HttpPostService(IOptionsSnapshot<HttpPostServiceOptions> options, ILogger<HttpPostService> logger, BotContext context)
    : BackgroundService, ILagrangeWebService
{
    private const string Tag = nameof(HttpPostService);

    public event EventHandler<MsgRecvEventArgs>? OnMessageReceived { add { } remove { } }

    private readonly HttpPostServiceOptions _options = options.Value;

    private readonly ILogger _logger = logger;

    private Uri? _url;

    private readonly HttpClient _client = new();
    
    private string ComputeSHA1(string data)
    {
        byte[] hash = HMACSHA1.HashData(Encoding.UTF8.GetBytes(data), Convert.FromHexString(_options.Secret));
        return Convert.ToHexString(hash).ToLower();
    }

    public async ValueTask SendJsonAsync<T>(T payload, string? identifier, CancellationToken cancellationToken = default)
    {
        if (_url is null) throw new InvalidOperationException("Reverse HTTP service was not running");

        if (payload is OneBotResult) return; // ignore api result

        string json = JsonSerializer.Serialize(payload);
        Log.LogSendingData(_logger, Tag, _url.ToString(), json);
        using var request = new HttpRequestMessage(HttpMethod.Post, _url)
        {
            Headers = { { "X-Self-ID", context.BotUin.ToString() } },
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        if (!string.IsNullOrEmpty(_options.Secret))
        {
            request.Headers.Add("X-Signature", $"sha1={ComputeSHA1(json)}");
        }

        try
        {
            await _client.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            Log.LogPostFailed(_logger, ex, Tag, _url.ToString());
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string urlstr = $"{_options.Host}:{_options.Port}{_options.Suffix}";
        if (!_options.Host.StartsWith("http://") && !_options.Host.StartsWith("https://"))
        {
            urlstr = "http://" + urlstr;
        }

        if (!Uri.TryCreate(urlstr, UriKind.Absolute, out _url))
        {
            Log.LogInvalidUrl(_logger, Tag, urlstr);
            return;
        }

        try
        {
            var lifecycle = new OneBotLifecycle(context.BotUin, "connect");
            await SendJsonAsync(lifecycle, null, stoppingToken);
            if (_options is { HeartBeatEnable: true, HeartBeatInterval: > 0 })
            {
                await HeartbeatLoop(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }
    }

    private async Task HeartbeatLoop(CancellationToken token)
    {
        var interval = TimeSpan.FromMilliseconds(_options.HeartBeatInterval);
        Stopwatch sw = new();

        while (true)
        {
            var status = new OneBotStatus(true, true);
            var heartBeat = new OneBotHeartBeat(context.BotUin, (int)_options.HeartBeatInterval, status);

            sw.Start();
            await SendJsonAsync(heartBeat, null, token);
            sw.Stop();

            // Implementing precise intervals by subtracting Stopwatch's timing from configured intervals
            var waitingTime = interval - sw.Elapsed;
            if (waitingTime >= TimeSpan.Zero)
            {
                await Task.Delay(waitingTime, token);
            }
            sw.Reset();
        }
    }

    private static partial class Log
    {
        private enum EventIds
        {
            SendingData = 1,

            PostFailed = 1001,
            InvalidUrl
        }

        [LoggerMessage(EventId = (int)EventIds.SendingData, Level = LogLevel.Trace, Message = "[{tag}] Send to {url}: {data}")]
        public static partial void LogSendingData(ILogger logger, string tag, string url, string data);

        [LoggerMessage(EventId = (int)EventIds.PostFailed, Level = LogLevel.Error, Message = "[{tag}] Post to {url} failed")]
        public static partial void LogPostFailed(ILogger logger, Exception ex, string tag, string url);

        [LoggerMessage(EventId = (int)EventIds.InvalidUrl, Level = LogLevel.Error, Message = "[{tag}] Invalid configuration was detected, url: {url}")]
        public static partial void LogInvalidUrl(ILogger logger, string tag, string url);
    }
}
