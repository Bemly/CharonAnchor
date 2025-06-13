using System.Threading.Tasks;
using Lagrange.Milky.Entity.Event;
using Lagrange.Milky.Extension;
using LgrEventArgs = Lagrange.Core.Events.EventArgs;

namespace Lagrange.Milky.Utility;

public partial class EntityConvert
{
    public BotOfflineEvent BotOfflineEvent(LgrEventArgs.BotOfflineEvent @event) => new(
        @event.EventTime.ToUnixTimeSeconds(),
        _bot.BotUin,
        new BotOfflineEventData($"{@event.Reason} {@event.Tips?.Tag} {@event.Tips?.Message}")
    );

    public async Task<MessageReceiveEvent> MessageReceiveEventAsync(LgrEventArgs.BotMessageEvent @event, CancellationToken token) => new(
        @event.Message.Time.ToUnixTimeSeconds(),
        _bot.BotUin,
        await MessageBaseAsync(@event.Message, token)
    );
}