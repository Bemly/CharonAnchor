using Lagrange.Core;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Event;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Lagrange.Milky.Implementation.Service;

public class EventService(ILogger<EventService> logger, BotContext bot, EventConvert @event) : IHostedService
{
    private readonly ILogger<EventService> _logger = logger;

    private readonly BotContext _bot = bot;

    private readonly EventConvert _event = @event;

    private readonly HashSet<Action<IEvent>> _handlers = [];
    private readonly ReaderWriterLockSlim _lock = new();

    private CancellationTokenSource? _cts;

    public Task StartAsync(CancellationToken token)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(token);

        _bot.EventInvoker.RegisterEvent<BotMessageEvent>(HandleMessageEvent);

        return Task.CompletedTask;
    }

    private void HandleMessageEvent(BotContext bot, BotMessageEvent @event)
    {
        try
        {
            var token = _cts?.Token ?? throw new Exception("_cts not initialized");

            var result = _event.ToIncomingMessageEvent(@event);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(result);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleMessageFailed(e);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // TODO: unregister
        // _bot.EventInvoker.UnregisterEvent<BotMessageEvent>(HandleMessageEvent);

        _cts?.Cancel();

        return Task.CompletedTask;
    }

    public void Register(Action<IEvent> handler)
    {
        using (_lock.UsingWriteLock())
        {
            _handlers.Add(handler);
        }
    }

    public void Unregister(Action<IEvent> handler)
    {
        using (_lock.UsingWriteLock())
        {
            _handlers.Remove(handler);
        }
    }
}

public static partial class EventServiceLoggerExtension
{
    [LoggerMessage(EventId = 999, Level = MSLogLevel.Error, Message = "Handle message failed")]
    public static partial void LogHandleMessageFailed(this ILogger<EventService> logger, Exception e);
}