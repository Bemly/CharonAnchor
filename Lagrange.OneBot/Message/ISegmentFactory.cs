using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;

namespace Lagrange.OneBot.Message;

public interface ISegmentFactory
{
    void Build(MessageBuilder builder, ISegment segment);
    
    ISegment Parse(BotMessage message, IMessageEntity entity);
}

public interface ISegmentFactory<TSegment, in TEntity> : ISegmentFactory
    where TSegment : ISegment
    where TEntity : IMessageEntity
{
    ISegment ISegmentFactory.Parse(BotMessage message, IMessageEntity entity) => Parse(message, (TEntity)entity);
    
    void ISegmentFactory.Build(MessageBuilder builder, ISegment segment) => Build(builder, (TSegment)segment);
    
    void Build(MessageBuilder builder, TSegment segment);

    TSegment Parse(BotMessage message, TEntity entity);
}