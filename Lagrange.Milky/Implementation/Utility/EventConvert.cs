using Lagrange.Core;
using Lagrange.Core.Events;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Milky.Implementation.Entity.Incoming.Message;
using Lagrange.Milky.Implementation.Event;

namespace Lagrange.Milky.Implementation.Utility;

public class EventConvert(BotContext bot, EntityConvert entity)
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _entity = entity;

    private Event<TData> CreateEvent<TData>(EventBase @event, string type, TData data) => new(type)
    {
        Time = new DateTimeOffset(@event.EventTime).ToUnixTimeSeconds(),
        SelfId = _bot.BotUin,
        Data = data,
    };

    public Event<IncomingMessageBase> ToIncomingMessageEvent(BotMessageEvent @event)
    {
        return CreateEvent(@event, "message_receive", _entity.ToIncomingMessage(@event.Message));
    }
}
