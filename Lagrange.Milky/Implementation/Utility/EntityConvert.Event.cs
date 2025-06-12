using Lagrange.Core.Message;
using Lagrange.Milky.Extension;
using Lagrange.Milky.Implementation.Entity.Event;
using LgrEventArgs = Lagrange.Core.Events.EventArgs;

namespace Lagrange.Milky.Implementation.Utility;

public partial class EntityConvert
{
    public BotOfflineEvent BotOfflineEvent(LgrEventArgs.BotOfflineEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new BotOfflineEventData($"{@event.Reason} {@event.Tips?.Tag} {@event.Tips?.Message}")
    );

    public MessageReceiveEvent MessageReceiveEvent(LgrEventArgs.BotMessageEvent @event) => new(
        @event.Message.Time.ToUnixTimeSeconds(),
        _bot.BotUin,
        MessageBase(@event.Message)
    );
}