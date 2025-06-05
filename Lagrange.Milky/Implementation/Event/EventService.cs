using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Configuration;
using Lagrange.Milky.Implementation.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lagrange.Milky.Implementation.Event;

public class EventService(ILogger<EventService> logger, IOptions<MilkyConfiguration> options, BotContext bot, Converter converter) : IHostedService
{
    private readonly ILogger<EventService> _logger = logger;

    private readonly bool _ignoreBotMessage = options.Value.IgnoreBotMessage;

    private readonly BotContext _bot = bot;

    private readonly Converter _converter = converter;

    private readonly HashSet<Action<Memory<byte>>> _handlers = [];
    private readonly ReaderWriterLockSlim _lock = new();

    public Task StartAsync(CancellationToken token)
    {
        _bot.EventInvoker.RegisterEvent<BotOfflineEvent>(HandleOfflineEvent);
        _bot.EventInvoker.RegisterEvent<BotMessageEvent>(HandleMessageEvent);

        return Task.CompletedTask;
    }

    private void HandleOfflineEvent(BotContext bot, BotOfflineEvent @event)
    {
        try
        {
            var result = _converter.ToBotOfflineEvent(@event);
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
            _logger.LogHandleEventException(nameof(BotOfflineEvent), e);
        }
    }

    private void HandleMessageEvent(BotContext bot, BotMessageEvent @event)
    {
        try
        {
            if (_ignoreBotMessage && @event.Message.Contact.Uin == bot.BotUin) return;

            var result = _converter.ToMessageReceiveEvent(@event);
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
            _logger.LogHandleEventException(nameof(BotMessageEvent), e);
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
    [LoggerMessage(EventId = 999, Level = Microsoft.Extensions.Logging.LogLevel.Error, Message = "Handle {event} exception")]
    public static partial void LogHandleEventException(this ILogger<EventService> logger, string @event, Exception e);
}