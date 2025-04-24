namespace Lagrange.Core.Internal.Events.Message;

internal class PushMessageEvent(ReadOnlyMemory<byte> raw) : ProtocolEvent
{
    public PushMessageEvent() : this(ReadOnlyMemory<byte>.Empty) { }

    internal ReadOnlyMemory<byte> Raw { get; } = raw;
}