using System.Net;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Extension;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Service;

public class MilkyService(ILogger<MilkyService> logger, IOptions<MilkyConfiguration> options, MilkyApiService api) : BackgroundService
{
    private readonly ILogger<MilkyService> _logger = logger;

    private readonly MilkyApiService _api = api;

    private readonly HttpListener _listener = new();

    public override Task StartAsync(CancellationToken token)
    {
        string host = options.Value.Host ?? throw new Exception("Milky.Host cannot be null");
        ulong port = options.Value.Port ?? throw new Exception("Milky.Port cannot be null");

        _listener.Prefixes.Add($"http://{host}:{port}{options.Value.Prefix}");
        _listener.Start();

        foreach (string prefix in _listener.Prefixes) _logger.LogServerStarted(prefix);

        return base.StartAsync(token);
    }

    protected override Task ExecuteAsync(CancellationToken token) => GetHttpContextLoopAsync(token);

    public override async Task StopAsync(CancellationToken token)
    {
        await base.StopAsync(token);

        _listener.Close();
    }

    private async Task GetHttpContextLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                _ = HandleHttpContextAsync(await _listener.GetContextAsync().WaitAsync(token), token);
            }
        }
        catch (TaskCanceledException) { }
        catch (Exception e)
        {
            _logger.LogGetHttpContextFailed(e);
            throw;
        }
    }

    private async Task HandleHttpContextAsync(HttpListenerContext httpContext, CancellationToken token)
    {

        var request = httpContext.Request;
        var identifier = request.RequestTraceIdentifier;

        try
        {
            string? path = request.Url?.LocalPath;
            _logger.LogHttpContext(identifier, request.RemoteEndPoint, request.HttpMethod, path);

            if (path == null)
            {
                httpContext.Response.Send(HttpStatusCode.NotFound);
                _logger.LogSend(identifier, HttpStatusCode.NotFound);
                return;
            }
            // Api
            if (_api.IsApiPath(path))
            {
                await _api.HandleAsync(httpContext, token);
                return;
            }

            // Fallback
            httpContext.Response.Send(HttpStatusCode.NotFound);
            _logger.LogSend(identifier, HttpStatusCode.NotFound);
        }
        catch (Exception e)
        {
            _logger.LogHandleHttpContextFailed(identifier, e);
            throw;
        }
    }
}

public static partial class MilkyServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "The server is started at {prefix}")]
    public static partial void LogServerStarted(this ILogger<MilkyService> logger, string prefix);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{identifier} {remote} >> {method} {path}")]
    public static partial void LogHttpContext(this ILogger<MilkyService> logger, Guid identifier, IPEndPoint remote, string method, string? path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "{identifier} << {status}")]
    public static partial void LogSend(this ILogger<MilkyService> logger, Guid identifier, HttpStatusCode status);

    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{identifier} >< Handle context failed")]
    public static partial void LogHandleHttpContextFailed(this ILogger<MilkyService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "Get context failed")]
    public static partial void LogGetHttpContextFailed(this ILogger<MilkyService> logger, Exception e);
}