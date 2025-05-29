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
        try
        {
            while (true)
            {
                _ = HandleHttpContextAsync(await _listener.GetContextAsync().WaitAsync(token), token);

                token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            _logger.LogGetHttpContextException(e);
            throw;
        }
    }

    private async Task HandleHttpContextAsync(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var identifier = request.RequestTraceIdentifier;
        var remote = request.RemoteEndPoint;
        string method = request.HttpMethod;
        string? rawUrl = request.RawUrl;

        try
        {
            _logger.LogHttpContext(identifier, remote, method, rawUrl);

            if (!await ValidateHttpContextAsync(httpContext, token)) return;

            var connection = await GetConnectionContextAsync(httpContext, token);
            if (connection == null) return;

            _ = WaitConnectionCloseLoopAsync(connection, connection.Cts.Token);
        }
        catch (OperationCanceledException)
        {
            await SendWithLoggerAsync(httpContext, HttpStatusCode.InternalServerError, token);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogHandleHttpContextException(identifier, remote, e);
            await SendWithLoggerAsync(httpContext, HttpStatusCode.InternalServerError, token);
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
        if (_connections.IsEmpty) return;

        _logger.LogSend(payload.Span);
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

    private async Task<bool> ValidateHttpContextAsync(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var identifier = request.RequestTraceIdentifier;
        var remote = request.RemoteEndPoint;

        if (request.Url?.LocalPath != _path)
        {
            await SendWithLoggerAsync(httpContext, HttpStatusCode.NotFound, token);
        }

        if (!httpContext.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            await SendWithLoggerAsync(httpContext, HttpStatusCode.MethodNotAllowed, token);
            return false;
        }

        if (!ValidateApiAccessToken(httpContext))
        {
            _logger.LogValidateAccessTokenFailed(identifier, remote);
            await SendWithLoggerAsync(httpContext, HttpStatusCode.Unauthorized, token);
            return false;
        }

        if (!request.IsWebSocketRequest)
        {
            await SendWithLoggerAsync(httpContext, HttpStatusCode.BadRequest, token);
            return false;
        }

        return true;
    }

    private bool ValidateApiAccessToken(HttpListenerContext httpContext)
    {
        if (_token == null) return true;

        string? authorization = httpContext.Request.QueryString["access_token"];
        if (authorization == null) return false;

        return authorization == _token;
    }

    private async Task<ConnectionContext?> GetConnectionContextAsync(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var identifier = request.RequestTraceIdentifier;
        var remote = request.RemoteEndPoint;

        try
        {
            var wsContext = await httpContext.AcceptWebSocketAsync(null).WaitAsync(token);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var connection = new ConnectionContext { HttpContext = httpContext, WsContext = wsContext, Cts = cts };
            _connections.TryAdd(connection, null);
            return connection;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            _logger.LogUpgradeWebSocketException(identifier, remote, e);
            await SendWithLoggerAsync(httpContext, HttpStatusCode.InternalServerError, token);
        }

        return null;
    }

    private async Task SendWithLoggerAsync(HttpListenerContext context, HttpStatusCode status, CancellationToken token)
    {
        var request = context.Request;
        var identifier = request.RequestTraceIdentifier;
        var remote = request.RemoteEndPoint;

        var response = context.Response;
        var output = response.OutputStream;

        try
        {
            int code = (int)status;

            response.StatusCode = code;
            await output.WriteAsync(Encoding.UTF8.GetBytes($"{code} {status}"), token);

            _logger.LogSend(identifier, remote, status);
        }
        catch (Exception e)
        {
            _logger.LogSendException(identifier, remote, e);
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

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "{identifier} {remote} -->> {method} {path}")]
    public static partial void LogHttpContext(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, string method, string? path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "{identifier} {remote} <<-- {status}")]
    public static partial void LogSend(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, HttpStatusCode status);

    [LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "WebSockets <<-- {payload}")]
    private static partial void LogSend(this ILogger<MilkyWebSocketEventService> logger, string payload);
    public static void LogSend(this ILogger<MilkyWebSocketEventService> logger, Span<byte> payload)
    {
        if (logger.IsEnabled(LogLevel.Debug)) logger.LogSend(Encoding.UTF8.GetString(payload));
    }

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "{identifier} {remote} <//> WebSocket closed")]
    public static partial void LogWebSocketClosed(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote);


    [LoggerMessage(EventId = 995, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> WebSocket close failed")]
    public static partial void LogWebSocketCloseException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);

    [LoggerMessage(EventId = 995, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Wait websocket close failed")]
    public static partial void LogWaitWebSocketCloseException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);

    [LoggerMessage(EventId = 995, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Send failed")]
    public static partial void LogSendException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);

    [LoggerMessage(EventId = 996, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Handle http context failed")]
    public static partial void LogHandleHttpContextException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);

    [LoggerMessage(EventId = 997, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Upgrade websocket failed")]
    public static partial void LogUpgradeWebSocketException(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote, Exception e);

    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Validate access token failed")]
    public static partial void LogValidateAccessTokenFailed(this ILogger<MilkyWebSocketEventService> logger, Guid identifier, IPEndPoint remote);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "Get http context failed")]
    public static partial void LogGetHttpContextException(this ILogger<MilkyWebSocketEventService> logger, Exception e);
}