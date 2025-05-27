using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Event;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Communication;

public class MilkyWebSocketEventService(ILogger<MilkyWebSocketEventService> logger, IOptions<MilkyConfiguration> options, EventService @event) : IHostedService
{
    private readonly ILogger<MilkyWebSocketEventService> _logger = logger;

    private readonly string _host = options.Value.Host ?? throw new Exception("Milky.Host cannot be null");
    private readonly ulong _port = options.Value.Port ?? throw new Exception("Milky.Port cannot be null");
    private readonly string _path = $"{options.Value.Prefix}{(options.Value.Prefix.EndsWith('/') ? "" : "/")}event";
    private readonly string? _token = options.Value.AccessToken;

    private readonly EventService _event = @event;

    private readonly HttpListener _listener = new();
    private readonly ConcurrentDictionary<ConnectionContext, object?> _connections = [];
    private CancellationTokenSource? _cts;
    private Task? _task;

    public Task StartAsync(CancellationToken token)
    {
        _listener.Prefixes.Add($"http://{_host}:{_port}{_path}/");
        _listener.Start();

        foreach (var prefix in _listener.Prefixes) _logger.LogServerRunning(prefix);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _task = GetHttpContextLoopAsync(_cts.Token);

        _event.Register(HandleEventAsync);

        return Task.CompletedTask;
    }

    private async Task GetHttpContextLoopAsync(CancellationToken token)
    {
        while (true)
        {
            _ = HandleHttpContextAsync(await _listener.GetContextAsync().WaitAsync(token), token);

            token.ThrowIfCancellationRequested();
        }
    }

    private async Task HandleHttpContextAsync(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var identifier = request.RequestTraceIdentifier;

        try
        {
            _logger.LogHttpContext(identifier, request.RemoteEndPoint, request.HttpMethod, request.RawUrl);

            if (request.Url?.LocalPath != _path)
            {
                await SendWithLoggerAsync(httpContext, HttpStatusCode.NotFound, LogLevel.Warning, token);
            }

            if (!request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                await SendWithLoggerAsync(httpContext, HttpStatusCode.MethodNotAllowed, LogLevel.Warning, token);
                return;
            }

            if (!ValidateApiAccessToken(httpContext))
            {
                await SendWithLoggerAsync(httpContext, HttpStatusCode.Unauthorized, LogLevel.Warning, token);
                return;
            }

            if (!request.IsWebSocketRequest)
            {
                await SendWithLoggerAsync(httpContext, HttpStatusCode.BadRequest, LogLevel.Warning, token);
                return;
            }

            var wsContext = await httpContext.AcceptWebSocketAsync(null).WaitAsync(token);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var connection = new ConnectionContext { HttpContext = httpContext, WsContext = wsContext, Cts = cts };
            _connections.TryAdd(connection, null);

            _ = WaitConnectionCloseLoopAsync(connection, cts.Token);
        }
        catch (Exception e)
        {
            await SendWithLoggerAsync(httpContext, HttpStatusCode.InternalServerError, LogLevel.Error, e, token);
        }
    }

    private async Task WaitConnectionCloseLoopAsync(ConnectionContext connection, CancellationToken token)
    {
        var identifier = connection.HttpContext.Request.RequestTraceIdentifier;
        var remote = connection.HttpContext.Request.RemoteEndPoint;

        try
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                ValueTask<ValueWebSocketReceiveResult> resultTask = connection.WsContext.WebSocket
                        .ReceiveAsync(buffer.AsMemory(), default);

                ValueWebSocketReceiveResult result = !resultTask.IsCompleted ?
                    await resultTask.AsTask().WaitAsync(token) :
                    resultTask.Result;

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseConnectionAsync(connection, WebSocketCloseStatus.NormalClosure, token);
                    return;
                }

                token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            await CloseConnectionAsync(connection, WebSocketCloseStatus.NormalClosure, default);
        }
        catch (Exception e)
        {
            _logger.LogWaitWebSocketCloseException(identifier, remote, e);

            await CloseConnectionAsync(connection, WebSocketCloseStatus.InternalServerError, token);
        }
    }

    private async Task CloseConnectionAsync(ConnectionContext connection, WebSocketCloseStatus status, CancellationToken token)
    {
        var identifier = connection.HttpContext.Request.RequestTraceIdentifier;
        var remote = connection.HttpContext.Request.RemoteEndPoint;

        try
        {
            _connections.Remove(connection, out _);

            await connection.WsContext.WebSocket.CloseAsync(status, null, token);

            _logger.LogWebSocketClosed(identifier, remote);
        }
        catch (Exception e)
        {
            _logger.LogWebSocketCloseException(identifier, remote, e);
        }
        finally
        {
            connection.Tcs.SetResult();
        }
    }

    private async void HandleEventAsync(Memory<byte> payload)
    {
        _logger.LogSend(payload);
        foreach (var connection in _connections.Keys)
        {
            var identifier = connection.HttpContext.Request.RequestTraceIdentifier;
            var remote = connection.HttpContext.Request.RemoteEndPoint;
            var ws = connection.WsContext.WebSocket;

            try
            {
                await connection.SendSemaphoreSlim.WaitAsync(connection.Cts.Token);
                try
                {
                    await ws.SendAsync(payload, WebSocketMessageType.Text, true, connection.Cts.Token);
                }
                finally
                {
                    connection.SendSemaphoreSlim.Release();
                }
            }
            catch (Exception e)
            {
                _logger.LogSendException(identifier, remote, e);

                await CloseConnectionAsync(connection, WebSocketCloseStatus.InternalServerError, connection.Cts.Token);
            }
        }
    }

    public async Task StopAsync(CancellationToken token)
    {
        _event.Unregister(HandleEventAsync);

        _cts?.Cancel();
        if (_task != null) await _task.WaitAsync(token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        await Task.WhenAll(_connections.Keys.Select(connection => connection.Tcs.Task));

        _listener.Stop();
    }

    private bool ValidateApiAccessToken(HttpListenerContext context)
    {
        if (_token == null) return true;

        string? authorization = context.Request.QueryString["access_token"];
        if (authorization == null) return false;

        return authorization == _token;
    }

    private Task SendWithLoggerAsync(HttpListenerContext context, HttpStatusCode status, LogLevel level, CancellationToken token)
    {
        return SendWithLoggerAsync(context, status, level, null, token);
    }
    private async Task SendWithLoggerAsync(HttpListenerContext context, HttpStatusCode status, LogLevel level, Exception? e, CancellationToken token)
    {
        try
        {
            context.Response.StatusCode = (int)status;
            await context.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes($"{(int)status} {status}"), token);
            context.Response.Close();

            _logger.LogSend(level, context.Request.RequestTraceIdentifier, context.Request.RemoteEndPoint, status, e);
        }
        catch (Exception ex)
        {
            Exception exc = e == null ? ex : new AggregateException(e, ex);
            _logger.LogSendException(context.Request.RequestTraceIdentifier, context.Request.RemoteEndPoint, exc);
        }
    }

    private class ConnectionContext
    {
        public required HttpListenerContext HttpContext { get; init; }
        public required WebSocketContext WsContext { get; init; }

        public SemaphoreSlim SendSemaphoreSlim { get; } = new(1);

        public required CancellationTokenSource Cts { get; init; }
        public TaskCompletionSource Tcs { get; } = new();
    }
}

public static partial class MilkyWebSocketEventServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Event websocket server is running on {prefix}")]
    public static partial void LogServerRunning(this ILogger<MilkyWebSocketEventService> logger, string prefix);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{identifier} {remote} -->> {method} {path}")]
    public static partial void LogHttpContext(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, string method, string? path);

    [LoggerMessage(EventId = 2, Message = "{identifier} {remote} <<-- {status}")]
    public static partial void LogSend(this ILogger<MilkyWebSocketEventService> logger, LogLevel level, Guid identifier, IPEndPoint remote, HttpStatusCode status, Exception? e);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{identifier} {remote} <//> WebSocket close")]
    public static partial void LogWebSocketClosed(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "ALL WEBSOCKET <<-- {payload}", SkipEnabledCheck = true)]
    private static partial void LogSend(this ILogger<MilkyWebSocketEventService> logger, string payload);
    public static void LogSend(this ILogger<MilkyWebSocketEventService> logger, Memory<byte> body)
    {
        if (logger.IsEnabled(LogLevel.Information)) logger.LogSend(Encoding.UTF8.GetString(body.Span));
    }


    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> WebSocket close exception")]
    public static partial void LogWebSocketCloseException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);

    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Wait websocket close exception")]
    public static partial void LogWaitWebSocketCloseException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Send exception")]
    public static partial void LogSendException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);
}