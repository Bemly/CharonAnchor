using Lagrange.Core.Message.Entities;

namespace Lagrange.OneBot.Message;

[AttributeUsage(AttributeTargets.Class)]
public class SegmentSubscriberAttribute<TEntity>(string type, string? sendType = null)
    : SegmentSubscriberAttribute(typeof(TEntity), type, sendType) where TEntity : IMessageEntity;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SegmentSubscriberAttribute(Type entity, string type, string? sendType = null) : Attribute
{
    public Type Entity { get; } = entity;

    public string Type { get; } = type;

    public string SendType { get; } = sendType ?? type;
}