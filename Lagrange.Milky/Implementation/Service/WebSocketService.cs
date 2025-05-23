using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using Lagrange.Milky.Implementation.Api;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Extension;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Lagrange.Milky.Implementation.Event;
using System.Collections.Concurrent;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Api.Parameter;

namespace Lagrange.Milky.Implementation.Service;

public class WebSocketService : BackgroundService
{
    private readonly ILogger<WebSocketService> _logger;

    private readonly string _host;
    private readonly ulong _port;
    private readonly string _prefix;
    private readonly string? _accessToken;

    private readonly string _apiPrefix;
    private readonly string _eventPath;

    private readonly IServiceProvider _services;
    private readonly EventService _event;

    private readonly HttpListener _listener = new();
    private readonly ConcurrentDictionary<ConnectionContext, object?> _connections = [];
    private CancellationTokenSource? _cts;

    public WebSocketService(ILogger<WebSocketService> logger, IOptions<MilkyConfiguration> options, IServiceProvider services, EventService @event)
    {
        _logger = logger;

        var configuration = options.Value.WebSocket;
        if (configuration == null) throw new Exception("Milky.WebSocket is null, this shouldn't happen");
        _host = configuration.Host ?? throw new Exception("Milky.WebSocket.Host cannot be null");
        _port = configuration.Port ?? throw new Exception("Milky.WebSocket.Port cannot be null");
        _prefix = configuration.Prefix;
        _accessToken = configuration.AccessToken;

        _apiPrefix = $"{_prefix}api/";
        _eventPath = $"{_prefix}event";

        _services = services;
        _event = @event;
    }

    #region base
    public override Task StartAsync(CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        _listener.Prefixes.Add($"http://{_host}:{_port}{_prefix}");
        _listener.Start();

        foreach (string prefix in _listener.Prefixes) _logger.LogServerStarted(prefix);

        _event.Register(HandleEventAsync);

        return base.StartAsync(token);
    }

    private async void HandleEventAsync(IEvent @event)
    {
        try
        {
            byte[] payload = MilkyJsonUtility.SerializeToUtf8Bytes(@event.GetType(), @event);
            var token = _cts?.Token ?? throw new Exception("_cts not initialized");

            await Task.WhenAll(_connections.Keys.Select(c => SendBytesAsync(c, payload, token)));
        }
        catch (Exception e)
        {
            _logger.LogHandleEventFailed(e);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken token)
    {
        try
        {
            while (true)
            {
                _ = HandleHttpListenerContext(await _listener.GetContextAsync().WaitAsync(token), token);

                token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogGetHttpContextFailed(e);
        }
    }

    public override async Task StopAsync(CancellationToken token)
    {
        await base.StopAsync(token);

        _event.Unregister(HandleEventAsync);

        await Task.WhenAll(_connections.Keys.Select(c => c.ClosedTcs.Task)).WaitAsync(token);

        _listener.Stop();
    }

    private async Task HandleHttpListenerContext(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var identifier = request.RequestTraceIdentifier;

        var path = request.Url?.LocalPath;

        try
        {
            _logger.LogHttpContext(identifier, request.RemoteEndPoint, request.HttpMethod, path);

            if (IsApiPath(path)) await HandleApiAsync(httpContext, token);
            else if (IsEventPath(path)) await HandleEventAsync(httpContext, token);
            else
            {
                response.Send(HttpStatusCode.NotFound);
                _logger.LogSend(identifier, HttpStatusCode.NotFound);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogHandleHttpContextFailed(identifier, e);
        }
    }
    #endregion

    #region Api
    private bool IsApiPath(string? path) => path?.StartsWith(_apiPrefix) ?? false;

    private async Task HandleApiAsync(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var identifier = request.RequestTraceIdentifier;

        if (request.HttpMethod != "POST")
        {
            response.Send(HttpStatusCode.MethodNotAllowed);
            _logger.LogSend(identifier, HttpStatusCode.MethodNotAllowed);
            return;
        }

        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out MediaTypeHeaderValue? type) || type.MediaType != "application/json")
        {
            response.Send(HttpStatusCode.UnsupportedMediaType);
            _logger.LogSend(identifier, HttpStatusCode.UnsupportedMediaType);
            return;
        }

        if (!ValidateApiAccessToken(httpContext))
        {
            response.Send(HttpStatusCode.Unauthorized);
            _logger.LogSend(identifier, HttpStatusCode.Unauthorized);
            return;
        }

        string? api = request.Url?.LocalPath?[_apiPrefix.Length..];
        if (api == null) throw new Exception("The path should not be null here");

        var handler = _services.GetKeyedService<IApiHandler>(api);
        if (handler == null)
        {
            response.Send(HttpStatusCode.NotFound);
            _logger.LogSend(identifier, HttpStatusCode.NotFound);
            return;
        }

        object? parameter;
        try
        {
            parameter = MilkyJsonUtility.Deserialize(handler.ParameterType, request.InputStream);
            if (parameter == null) throw new NullReferenceException();
        }
        catch (Exception e)
        {
            _logger.LogDeserializeApiParameterFailed(identifier, e);

            var result = IApiResult.Failed(-400, "Parameter serialize failed");
            byte[] body = MilkyJsonUtility.SerializeToUtf8Bytes(typeof(ApiFailedResult), result);
            await response.SendJsonAsync(body, token);
            return;
        }

        try
        {
            IApiResult result = await handler.HandleAsync((IApiParameter)parameter, token);
            byte[] body = MilkyJsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            await response.SendJsonAsync(body, token);
            _logger.LogSend(identifier, body);
        }
        catch (Exception e)
        {
            _logger.LogHandleApiFailed(identifier, e);

            response.Send(HttpStatusCode.InternalServerError);
            _logger.LogSend(identifier, HttpStatusCode.InternalServerError);
            return;
        }
    }

    private bool ValidateApiAccessToken(HttpListenerContext httpContext)
    {
        if (_accessToken == null) return true;

        string? authorization = httpContext.Request.Headers["Authorization"];
        if (authorization == null) return false;
        if (!authorization.StartsWith("Bearer ")) return false;

        return authorization[7..] == _accessToken;
    }
    #endregion

    #region Event
    private bool IsEventPath(string? path) => path == _eventPath;

    private async Task HandleEventAsync(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var identifier = request.RequestTraceIdentifier;

        if (!httpContext.Request.IsWebSocketRequest)
        {
            response.Send(HttpStatusCode.MethodNotAllowed);
            _logger.LogSend(identifier, HttpStatusCode.MethodNotAllowed);
            return;
        }

        if (!ValidateEventAccessToken(httpContext))
        {
            response.Send(HttpStatusCode.Unauthorized);
            _logger.LogSend(identifier, HttpStatusCode.Unauthorized);
            return;
        }

        var wsContext = await httpContext.AcceptWebSocketAsync(null).WaitAsync(token);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var connection = new ConnectionContext(httpContext, wsContext, cts);
        _connections.TryAdd(connection, null);

        _ = WaitCloseAsyncLoop(connection, cts.Token);
    }

    private async Task WaitCloseAsyncLoop(ConnectionContext connection, CancellationToken token)
    {
        var ws = connection.WebSocketContext.WebSocket;

        try
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                ValueTask<ValueWebSocketReceiveResult> resultTask = ws.ReceiveAsync(buffer.AsMemory(), default);

                ValueWebSocketReceiveResult result = !resultTask.IsCompleted
                    ? await resultTask.AsTask().WaitAsync(token)
                    : resultTask.Result;

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await CloseConnectionAsync(connection, WebSocketCloseStatus.NormalClosure, token);
                    break;
                }

                token.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            await CloseConnectionAsync(connection, WebSocketCloseStatus.NormalClosure, default);
        }
        catch (Exception e)
        {
            _logger.LogWaitCloseWebSocketFailed(connection.HttpContext.Request.RequestTraceIdentifier, e);

            await CloseConnectionAsync(connection, WebSocketCloseStatus.InternalServerError, token);
        }
    }

    private async Task CloseConnectionAsync(ConnectionContext connection, WebSocketCloseStatus status, CancellationToken token)
    {
        try
        {
            _connections.Remove(connection, out _);

            await connection.WebSocketContext.WebSocket.CloseAsync(status, null, token);

            _logger.LogWebSocketClosed(connection.HttpContext.Request.RequestTraceIdentifier);
        }
        catch (Exception e)
        {
            _logger.LogCloseWebSocketFailed(connection.HttpContext.Request.RequestTraceIdentifier, e);
        }
        finally
        {
            connection.ClosedTcs.SetResult();
            connection.Dispose();
        }
    }

    private bool ValidateEventAccessToken(HttpListenerContext httpContext)
    {
        if (_accessToken == null) return true;

        return httpContext.Request.QueryString["access_token"] == _accessToken;
    }
    #endregion

    #region WebSocket
    public async Task SendBytesAsync(ConnectionContext connection, byte[] payload, CancellationToken token)
    {
        var ws = connection.WebSocketContext.WebSocket;

        var identifier = connection.HttpContext.Request.RequestTraceIdentifier;

        await connection.SendSemaphoreSlim.WaitAsync(token);

        try
        {
            _logger.LogSend(identifier, payload);
            await ws.SendAsync(payload.AsMemory(), WebSocketMessageType.Text, true, token);
        }
        finally
        {
            connection.SendSemaphoreSlim.Release();
        }
    }
    #endregion

    public class ConnectionContext(HttpListenerContext httpContext, HttpListenerWebSocketContext webSocketContext, CancellationTokenSource cts) : IDisposable
    {
        public HttpListenerContext HttpContext { get; } = httpContext;
        public HttpListenerWebSocketContext WebSocketContext { get; } = webSocketContext;
        public SemaphoreSlim SendSemaphoreSlim { get; } = new(1);
        public CancellationTokenSource Cts { get; } = cts;
        public TaskCompletionSource ClosedTcs { get; } = new();

        public void Dispose()
        {
            SendSemaphoreSlim.Dispose();
            Cts.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}

public static partial class WebSocketServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "The server is started at {prefix}")]
    public static partial void LogServerStarted(this ILogger<WebSocketService> logger, string prefix);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{identifier} {remote} >> {method} {path}")]
    public static partial void LogHttpContext(this ILogger<WebSocketService> logger, Guid identifier, IPEndPoint remote, string method, string? path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "{identifier} << {status}")]
    public static partial void LogSend(this ILogger<WebSocketService> logger, Guid identifier, HttpStatusCode status);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{identifier} << {body}", SkipEnabledCheck = true)]
    private static partial void LogSend(this ILogger<WebSocketService> logger, Guid identifier, string body);
    public static void LogSend(this ILogger<WebSocketService> logger, Guid identifier, ReadOnlySpan<byte> body)
    {
        if (logger.IsEnabled(LogLevel.Information)) logger.LogSend(identifier, Encoding.UTF8.GetString(body));
    }

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "{identifier} // WebSocket closed")]
    public static partial void LogWebSocketClosed(this ILogger<WebSocketService> logger, Guid identifier);


    [LoggerMessage(EventId = 995, Level = LogLevel.Error, Message = "Handle event failed")]
    public static partial void LogHandleEventFailed(this ILogger<WebSocketService> logger, Exception e);

    [LoggerMessage(EventId = 995, Level = LogLevel.Error, Message = "{identifier} !! Wait close WebSocket failed")]
    public static partial void LogWaitCloseWebSocketFailed(this ILogger<WebSocketService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 995, Level = LogLevel.Error, Message = "{identifier} !! Close WebSocket failed")]
    public static partial void LogCloseWebSocketFailed(this ILogger<WebSocketService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 996, Level = LogLevel.Error, Message = "{identifier} !! Handle api failed")]
    public static partial void LogHandleApiFailed(this ILogger<WebSocketService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 997, Level = LogLevel.Error, Message = "{identifier} !! Deserialize api parameter failed")]
    public static partial void LogDeserializeApiParameterFailed(this ILogger<WebSocketService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{identifier} !! Handle context failed")]
    public static partial void LogHandleHttpContextFailed(this ILogger<WebSocketService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "Get context failed")]
    public static partial void LogGetHttpContextFailed(this ILogger<WebSocketService> logger, Exception e);
}