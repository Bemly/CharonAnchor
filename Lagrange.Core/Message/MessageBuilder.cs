using Lagrange.Core.Message.Entities;

namespace Lagrange.Core.Message;

public class MessageBuilder
{
    private readonly List<IMessageEntity> _entities = [];

    public MessageChain Build() => [.._entities];

    public MessageBuilder Text(string text)
    {
        _entities.Add(new TextEntity(text));
        return this;
    }
    
    public static MessageBuilder operator +(MessageBuilder builder, IMessageEntity entity)
    {
        builder._entities.Add(entity);
        return builder;
    }
    
    public static MessageBuilder operator +(MessageBuilder self, MessageBuilder other)
    {
        self._entities.AddRange(other._entities);
        return self;
    }
}