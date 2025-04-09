using Lagrange.Core.Message.Entities;

namespace Lagrange.OneBot.Message;

[AttributeUsage(AttributeTargets.Class)]
public class SegmentSubscriberAttribute<T>(string type, string? sendType = null)
    : SegmentSubscriberAttribute(typeof(T), type, sendType) where T : IMessageEntity;

[AttributeUsage(AttributeTargets.Class)]
public class SegmentSubscriberAttribute(Type entity, string type, string? sendType = null) : Attribute
{
    public Type Entity { get; } = entity;

    public string Type { get; } = type;

    public string SendType { get; } = sendType ?? type;
}