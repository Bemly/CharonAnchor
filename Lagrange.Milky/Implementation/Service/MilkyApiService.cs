using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Lagrange.Milky.Implementation.Api;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Extension;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Service;

public class MilkyApiService(ILogger<MilkyApiService> logger, IOptions<MilkyConfiguration> options, IServiceProvider service)
{
    private readonly ILogger<MilkyApiService> _logger = logger;

    private readonly IServiceProvider _services = service;

    private readonly string _pathPrefix = $"{options.Value.Prefix}api/";

    private readonly string? _accessToken = options.Value.AccessToken;

    public bool IsApiPath(string path) => path.StartsWith(_pathPrefix);

    public async Task HandleAsync(HttpListenerContext httpContext, CancellationToken token)
    {
        var request = httpContext.Request;
        var response = httpContext.Response;

        var identifier = request.RequestTraceIdentifier;

        if (request.HttpMethod != "POST")
        {
            response.Send(HttpStatusCode.MethodNotAllowed);
            _logger.LogSend(identifier, HttpStatusCode.NotFound);
            return;
        }

        if (!MediaTypeHeaderValue.TryParse(request.ContentType, out MediaTypeHeaderValue? type) || type.MediaType != "application/json")
        {
            response.Send(HttpStatusCode.MethodNotAllowed);
            _logger.LogSend(identifier, HttpStatusCode.MethodNotAllowed);
            return;
        }

        if (!ValidateAccessToken(httpContext))
        {
            response.Send(HttpStatusCode.Forbidden);
            _logger.LogSend(identifier, HttpStatusCode.Forbidden);
            return;
        }

        string? api = request.Url?.LocalPath?[_pathPrefix.Length..];
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

            response.Send(HttpStatusCode.BadRequest);
            _logger.LogSend(identifier, HttpStatusCode.BadRequest);
            return;
        }

        IApiResult result;
        try
        {
            result = await handler.HandleAsync(parameter, token);
        }
        catch (Exception e)
        {
            _logger.LogHandleApiFailed(identifier, e);

            response.Send(HttpStatusCode.InternalServerError);
            _logger.LogSend(identifier, HttpStatusCode.InternalServerError);
            return;
        }

        byte[] body = MilkyJsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
        await response.SendJsonAsync(body, token);
        _logger.LogSend(identifier, body);
    }

    private bool ValidateAccessToken(HttpListenerContext httpContext)
    {
        if (_accessToken == null) return true;

        string? authorization = httpContext.Request.Headers["Authorization"];
        if (authorization == null) return false;
        if (!authorization.StartsWith("Bearer ")) return false;

        return authorization[7..] == _accessToken;
    }
}

public static partial class MilkyApiServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "{identifier} << {status}")]
    public static partial void LogSend(this ILogger<MilkyApiService> logger, Guid identifier, HttpStatusCode status);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{identifier} << {body}", SkipEnabledCheck = true)]
    private static partial void LogSend(this ILogger<MilkyApiService> logger, Guid identifier, string body);
    public static void LogSend(this ILogger<MilkyApiService> logger, Guid identifier, ReadOnlySpan<byte> body)
    {
        if (logger.IsEnabled(LogLevel.Information)) logger.LogSend(identifier, Encoding.UTF8.GetString(body));
    }

    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{identifier} >< Handle api failed")]
    public static partial void LogHandleApiFailed(this ILogger<MilkyApiService> logger, Guid identifier, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{identifier} >< Deserialize api parameter failed")]
    public static partial void LogDeserializeApiParameterFailed(this ILogger<MilkyApiService> logger, Guid identifier, Exception e);
}