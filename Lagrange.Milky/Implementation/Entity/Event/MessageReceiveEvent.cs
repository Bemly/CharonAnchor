using Lagrange.Milky.Implementation.Entity.Message;

namespace Lagrange.Milky.Implementation.Entity.Event;

public class MessageReceiveEvent(long time, long selfId, MessageBase data) : EventBase<MessageBase>(time, selfId, "message_receive", data) { }
