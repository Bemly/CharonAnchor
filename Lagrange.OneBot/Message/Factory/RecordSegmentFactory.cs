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
        throw new NotImplementedException();
    }

    public RecordSegment Parse(BotMessage message, RecordEntity entity)
    {
        return new RecordSegment(entity.FileUrl);
    }
}