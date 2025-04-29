using Lagrange.Core.Common;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;

namespace Lagrange.Core.Internal.Logic;

[EventSubscribe<PushMessageEvent>(Protocols.All)]
internal class PushLogic(BotContext context) : ILogic
{
    public async ValueTask Incoming(ProtocolEvent e)
    {
        var messageEvent = (PushMessageEvent)e;

        switch ((Type)messageEvent.MsgPush.CommonMessage.ContentHead.Type)
        {
            case Type.GroupMessage:
            case Type.PrivateMessage:
            case Type.TempMessage:
                var message = await context.MessagePacker.Parse(messageEvent.MsgPush);
                context.EventInvoker.PostEvent(new BotMessageEvent(message, messageEvent.Raw));
                break;
        }
    }

    private enum Type
    {
        PrivateMessage = 166,
        GroupMessage = 82,
        TempMessage = 141,
    }
}