using Lagrange.Milky.Entity.Message;

namespace Lagrange.Milky.Entity.Event;

public class MessageReceiveEvent(long time, long selfId, MessageBase data) : EventBase<MessageBase>(time, selfId, "message_receive", data) { }