using Lagrange.Core.Common;

namespace Lagrange.Core.Internal.Events;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class EventSubscribeAttribute<T>(Protocols protocol) : EventSubscribeAttribute(typeof(T), protocol) where T : ProtocolEvent;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal class EventSubscribeAttribute(Type type, Protocols protocol) : Attribute
{
    public Type EventType { get; } = type;
    
    public Protocols Protocol { get; set; } = protocol;
}