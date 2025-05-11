using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.OneBot.Message.Entity;

namespace Lagrange.OneBot.Message.Factory;

[SegmentSubscriber<RecordEntity>("record")]
public class RecordSegmentFactory : ISegmentFactory<RecordSegment, RecordEntity>
{
    public Type SegmentType => typeof(RecordSegment);
    
    public void Build(MessageBuilder builder, RecordSegment segment)
    {
        if (CommonResolver.ResolveStream(segment.File) is { } stream)
        {
            builder.Record(stream);
        }
    }

    public RecordSegment Parse(BotMessage message, RecordEntity entity)
    {
        return new RecordSegment(entity.FileUrl);
    }
}