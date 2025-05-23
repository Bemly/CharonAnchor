using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Entity.Message.Incoming;

namespace Lagrange.Milky.Implementation.Event;

[JsonDerivedType(typeof(Event<IncomingMessageBase>))]
public interface IEvent
{
    long Time { get; }

    long SelfId { get; }

    string EventType { get; }

    object? Data { get; }
}