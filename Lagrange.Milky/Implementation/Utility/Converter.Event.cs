using Lagrange.Core.Events;
using Lagrange.Core.Events.EventArgs;

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

    public Event.Event ToIncomingMessageEvent(BotMessageEvent @event)
    {
        return CreateEvent(@event, "message_receive", ToIncomingMessage(@event.Message));
    }
}
