using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.OneBot.Message.Entity;

namespace Lagrange.OneBot.Message.Factory;

[SegmentSubscriber<MentionEntity>("at")]
public class AtSegmentFactory : ISegmentFactory<AtSegment, MentionEntity>
{
    public Type SegmentType => typeof(AtSegment);
    
    public virtual void Build(MessageBuilder builder, AtSegment segment)
    {
        throw new NotImplementedException();
    }

    public virtual AtSegment Parse(BotMessage message, MentionEntity entity)
    {
        return new AtSegment(entity.Uin, entity.Display);
    }
}