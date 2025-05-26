using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Event;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Communication;

public class MilkyWebHookEventService(ILogger<MilkyWebHookEventService> logger, IOptions<MilkyConfiguration> options, EventService @event) : IHostedService
{
    private readonly ILogger<MilkyWebHookEventService> _logger = logger;

    private readonly string _url = options.Value.WebHook?.Url ?? throw new Exception("Milky.WebHook.Url cannot be null");

    private readonly EventService _event = @event;

    private readonly HttpClient _client = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _event.Register(HandleEventAsync);

        _logger.LogServiceRunning(_url);

        return Task.CompletedTask;
    }

    private async void HandleEventAsync(Memory<byte> body)
    {
        try
        {
            _logger.LogSend(_url, body);

            using HttpRequestMessage request = new();
            request.Method = HttpMethod.Post;
            request.RequestUri = new Uri(_url);
            var content = new ReadOnlyMemoryContent(body);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json", "utf-8");
            request.Content = new ReadOnlyMemoryContent(body);

            using var response = await _client.SendAsync(request);

            if (response.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception($"Unexpected http status code({response.StatusCode})");
            }
        }
        catch (Exception e)
        {
            _logger.LogSendException(_url, e);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _event.Unregister(HandleEventAsync);

        return Task.CompletedTask;
    }
}

public static partial class MilkyWebHookEventServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "WebHook service running; delivering to {url}")]
    public static partial void LogServiceRunning(this ILogger<MilkyWebHookEventService> logger, string url);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{url} <<-- {body}", SkipEnabledCheck = true)]
    private static partial void LogSend(this ILogger<MilkyWebHookEventService> logger, string url, string body);
    public static void LogSend(this ILogger<MilkyWebHookEventService> logger, string url, Memory<byte> body)
    {
        if (logger.IsEnabled(LogLevel.Information)) logger.LogSend(url, Encoding.UTF8.GetString(body.Span));
    }


    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{url} <!!> Send exception")]
    public static partial void LogSendException(this ILogger<MilkyWebHookEventService> logger, string url, Exception e);
}