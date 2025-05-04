using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.OneBot.Message.Entity;

namespace Lagrange.OneBot.Message.Factory;

[SegmentSubscriber<TextEntity>("text")]
public class TextSegmentFactory : ISegmentFactory<TextSegment, TextEntity>
{
    public Type SegmentType => typeof(TextSegment);

    public virtual void Build(MessageBuilder builder, TextSegment segment)
    {
        builder.Text(segment.Text);
    }

    public virtual TextSegment Parse(BotMessage message, TextEntity entity)
    {
        return new TextSegment(entity.Text);
    }
}