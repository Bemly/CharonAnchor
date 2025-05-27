using System.Net;
using System.Text;
using Lagrange.Core.Exceptions;
using Lagrange.Milky.Implementation.Api;
using Lagrange.Milky.Implementation.Api.Exception;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Communication;

public class MilkyHttpApiService(ILogger<MilkyHttpApiService> logger, IOptions<MilkyConfiguration> options, IServiceProvider services) : IHostedService
{
    private readonly ILogger<MilkyHttpApiService> _logger = logger;

    private readonly string _host = options.Value.Host ?? throw new Exception("Milky.Host cannot be null");
    private readonly ulong _port = options.Value.Port ?? throw new Exception("Milky.Port cannot be null");
    private readonly string _prefix = $"{options.Value.Prefix}{(options.Value.Prefix.EndsWith('/') ? "" : "/")}api";
    private readonly string? _token = options.Value.AccessToken;

    private readonly IServiceProvider _services = services;

    private readonly HttpListener _listener = new();
    private CancellationTokenSource? _cts;
    private Task? _task;

    public Task StartAsync(CancellationToken token)
    {
        _listener.Prefixes.Add($"http://{_host}:{_port}{_prefix}/");
        _listener.Start();

        foreach (var prefix in _listener.Prefixes) _logger.LogServerRunning(prefix);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        _task = GetHttpContextLoopAsync(_cts.Token);

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

    private async Task HandleHttpContextAsync(HttpListenerContext context, CancellationToken token)
    {
        var request = context.Request;
        var identifier = request.RequestTraceIdentifier;

        try
        {
            _logger.LogHttpContext(identifier, request.RemoteEndPoint, request.HttpMethod, request.RawUrl);

            if (!request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                await SendWithLoggerAsync(context, HttpStatusCode.MethodNotAllowed, LogLevel.Warning, token);
                return;
            }

            if (!ValidateApiAccessToken(context))
            {
                await SendWithLoggerAsync(context, HttpStatusCode.Unauthorized, LogLevel.Warning, token);
                return;
            }

            string? path = request.Url?.LocalPath;
            string? api = path?.Length > _prefix.Length + 1 ? path[(_prefix.Length + 1)..] : null;
            var handler = _services.GetKeyedService<IApiHandler>(api);
            if (handler == null)
            {
                await SendWithLoggerAsync(context, HttpStatusCode.NotFound, LogLevel.Warning, token);
                return;
            }

            object? parameter;
            try
            {
                parameter = await MilkyJsonUtility.DeserializeAsync(
                    handler.ParameterType,
                    request.InputStream,
                    token
                );
                if (parameter == null) throw new NullReferenceException();
            }
            catch (Exception e)
            {
                await SendWithLoggerAsync(
                    context,
                    new ApiFailedResult { Retcode = -400, Message = "parameter deserialize failed" },
                    LogLevel.Warning,
                    e,
                    token
                );
                return;
            }

            object result;
            try
            {
                result = await handler.HandleAsync(parameter, token);
            }
            catch (ApiException e)
            {
                await SendWithLoggerAsync(
                    context,
                    new ApiFailedResult { Retcode = e.Retcode, Message = e.Error },
                    LogLevel.Warning,
                    token
                );
                return;
            }
            catch (OperationException e)
            {
                await SendWithLoggerAsync(
                    context,
                    new ApiFailedResult { Retcode = e.Result, Message = e.ErrMsg ?? string.Empty },
                    LogLevel.Error,
                    e,
                    token
                );
                return;
            }
            catch (Exception e)
            {
                await SendWithLoggerAsync(
                    context,
                    new ApiFailedResult { Retcode = -500, Message = "InternalServerError" },
                    LogLevel.Error,
                    e,
                    token
                );
                return;
            }

            await SendWithLoggerAsync(context, result, LogLevel.Information, token);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception e)
        {
            await SendWithLoggerAsync(context, HttpStatusCode.InternalServerError, LogLevel.Error, e, token);
        }
    }

    public async Task StopAsync(CancellationToken token)
    {
        _cts?.Cancel();
        if (_task != null) await _task.WaitAsync(token).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        _listener.Stop();
    }

    private bool ValidateApiAccessToken(HttpListenerContext context)
    {
        if (_token == null) return true;

        string? authorization = context.Request.Headers["Authorization"];
        if (authorization == null) return false;
        if (!authorization.StartsWith("Bearer ")) return false;

        return authorization[7..] == _token;
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

    private Task SendWithLoggerAsync<TBody>(HttpListenerContext context, TBody body, LogLevel level, CancellationToken token) where TBody : notnull
    {
        return SendWithLoggerAsync(context, body, level, null, token);
    }
    private async Task SendWithLoggerAsync<TBody>(HttpListenerContext context, TBody body, LogLevel level, Exception? e, CancellationToken token) where TBody : notnull
    {
        try
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            byte[] bytes = MilkyJsonUtility.SerializeToUtf8Bytes(body.GetType(), body);
            await context.Response.OutputStream.WriteAsync(bytes, token);
            context.Response.Close();

            _logger.LogSend(level, context.Request.RequestTraceIdentifier, context.Request.RemoteEndPoint, bytes, e);
        }
        catch (Exception ex)
        {
            Exception exc = e == null ? ex : new AggregateException(e, ex);
            _logger.LogSendException(context.Request.RequestTraceIdentifier, context.Request.RemoteEndPoint, exc);
        }
    }
}

public static partial class MilkyApiServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Api http server is running on {prefix}")]
    public static partial void LogServerRunning(this ILogger<MilkyHttpApiService> logger, string prefix);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{identifier} {remote} -->> {method} {path}")]
    public static partial void LogHttpContext(this ILogger<MilkyHttpApiService> logger, Guid identifier, IPEndPoint remote, string method, string? path);

    [LoggerMessage(EventId = 2, Message = "{identifier} {remote} <<-- {status}")]
    public static partial void LogSend(this ILogger<MilkyHttpApiService> logger, LogLevel level, Guid identifier, IPEndPoint remote, HttpStatusCode status, Exception? e);

    [LoggerMessage(EventId = 3, Message = "{identifier} {remote} <<-- {body}", SkipEnabledCheck = true)]
    private static partial void LogSend(this ILogger<MilkyHttpApiService> logger, LogLevel level, Guid identifier, IPEndPoint remote, string body, Exception? e);
    public static void LogSend(this ILogger<MilkyHttpApiService> logger, LogLevel level, Guid identifier, IPEndPoint remote, byte[] body, Exception? e)
    {
        if (logger.IsEnabled(level)) logger.LogSend(level, identifier, remote, Encoding.UTF8.GetString(body), e);
    }

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{identifier} {remote} <!!> Send exception")]
    public static partial void LogSendException(this ILogger<MilkyHttpApiService> logger, Guid identifier, IPEndPoint remote, Exception e);
}