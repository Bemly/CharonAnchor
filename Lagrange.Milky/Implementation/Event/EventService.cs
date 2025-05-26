using Lagrange.Core;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Implementation.Event;

public class EventService(ILogger<EventService> logger, BotContext bot, Converter converter) : IHostedService
{
    private readonly ILogger<EventService> _logger = logger;

    private readonly BotContext _bot = bot;

    private readonly Converter _converter = converter;

    private readonly HashSet<Action<Memory<byte>>> _handlers = [];
    private readonly ReaderWriterLockSlim _lock = new();

    public Task StartAsync(CancellationToken token)
    {
        _bot.EventInvoker.RegisterEvent<BotMessageEvent>(HandleMessageEvent);

        return Task.CompletedTask;
    }

    private void HandleMessageEvent(BotContext bot, BotMessageEvent @event)
    {
        try
        {
            var result = _converter.ToIncomingMessageEvent(@event);
            byte[] bytes = MilkyJsonUtility.SerializeToUtf8Bytes(result.GetType(), result);
            using (_lock.UsingReadLock())
            {
                foreach (var handler in _handlers)
                {
                    handler(bytes);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogHandleMessageException(e);
        }
    }

    public Task StopAsync(CancellationToken token)
    {
        // TODO: unregister
        // _bot.EventInvoker.UnregisterEvent<BotMessageEvent>(HandleMessageEvent);

        return Task.CompletedTask;
    }

    public void Register(Action<Memory<byte>> handler)
    {
        using (_lock.UsingWriteLock())
        {
            _handlers.Add(handler);
        }
    }

    public void Unregister(Action<Memory<byte>> handler)
    {
        using (_lock.UsingWriteLock())
        {
            _handlers.Remove(handler);
        }
    }
}

public static partial class EventServiceLoggerExtension
{
    [LoggerMessage(EventId = 999, Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Handle message exception")]
    public static partial void LogHandleMessageException(this ILogger<EventService> logger, Exception e);
}