using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Core;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Core.Extension;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;
using CoreLogLevel = Lagrange.Core.Events.EventArgs.LogLevel;

namespace Lagrange.Milky.Core.Service;

public partial class CoreLoggerService(ILogger<CoreLoggerService> logger, IOptionsMonitor<LoggerFilterOptions> loggerOptions, ILoggerFactory loggerFactory, BotContext bot) : IHostedService, IDisposable
{
    private readonly ILogger<CoreLoggerService> _logger = logger;
    private readonly IOptionsMonitor<LoggerFilterOptions> _loggerOptions = loggerOptions;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;
    private readonly BotContext _bot = bot;

    private readonly ConcurrentDictionary<string, ILogger> _loggers = [];

    private IDisposable? _levelChangeHandlerDisposable;

    public Task StartAsync(CancellationToken token)
    {
        _bot.EventInvoker.RegisterEvent<BotLogEvent>(HandleLog);

        _levelChangeHandlerDisposable = _loggerOptions.OnChange(HandleLoggerOptionsChange);

        return Task.CompletedTask;
    }

    private void HandleLoggerOptionsChange(LoggerFilterOptions options)
    {
        _bot.Config.LogLevel = (CoreLogLevel)options.GetDefaultLogLevel();

        _logger.LogCoreMinimumLogLevelReloaded(_bot.Config.LogLevel);
    }

    private void HandleLog(BotContext bot, BotLogEvent @event)
    {
        var logger = _loggers.GetOrAdd(@event.Tag, _loggerFactory.CreateLogger(InferFullName(@event.Tag)));
        LoggerUtility.LogBotMessage(logger, (MSLogLevel)@event.Level, @event.Message);
    }

    public Task StopAsync(CancellationToken token)
    {
        // TODO: unregister
        // _bot.EventInvoker.RegisterEvent<BotLogEvent>(HandleLog);

        _levelChangeHandlerDisposable?.Dispose();

        return Task.CompletedTask;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    private static string InferFullName(string tag)
    {
        foreach (var type in typeof(BotContext).Assembly.GetTypes())
        {
            if (type.Name == tag) return type.FullName ?? type.Name;
        }

        return tag;
    }

    public void Dispose()
    {
        _levelChangeHandlerDisposable?.Dispose();

        GC.SuppressFinalize(this);
    }

    private static partial class LoggerUtility
    {
        [LoggerMessage(EventId = 0, Message = "{message}")]
        public static partial void LogBotMessage(ILogger logger, MSLogLevel level, string message);
    }
}

public static partial class CoreLoggerServiceLoggerExtension
{
    [LoggerMessage(EventId = 0, Level = MSLogLevel.Information, Message = "Core minimum log level reloaded to {level}")]
    public static partial void LogCoreMinimumLogLevelReloaded(this ILogger<CoreLoggerService> logger, CoreLogLevel level);
}