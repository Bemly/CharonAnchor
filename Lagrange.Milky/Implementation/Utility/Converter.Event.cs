using Lagrange.Core.Events;
using Lagrange.Core.Events.EventArgs;
using MilkyBotOfflineEvent = Lagrange.Milky.Implementation.Entity.Event.BotOfflineEvent;

namespace Lagrange.Milky.Implementation.Utility;

public partial class Converter
{
    private Event.Event CreateEvent(EventBase @event, string type, object data) => new()
    {
        Time = new DateTimeOffset(@event.EventTime).ToUnixTimeSeconds(),
        SelfId = _bot.BotUin,
        EventType = type,
        Data = data,
    };

    public Event.Event ToBotOfflineEvent(BotOfflineEvent @event)
    {
        string reason;
        if (@event.Tips.HasValue) reason = $"({@event.Tips.Value.Tag}) {@event.Tips.Value.Message}";
        else reason = @event.Reason.ToString();

        return CreateEvent(@event, "bot_offline", new MilkyBotOfflineEvent { Reason = reason });
    }

    public Event.Event ToMessageReceiveEvent(BotMessageEvent @event)
    {
        return CreateEvent(@event, "message_receive", ToIncomingMessage(@event.Message));
    }
}
