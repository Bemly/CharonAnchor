using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.OneBot.Message.Entity;

namespace Lagrange.OneBot.Message.Factory;

[SegmentSubscriber<ImageEntity>("image")]
public class ImageSegmentFactory : ISegmentFactory<ImageSegment, ImageEntity>
{
    public Type SegmentType => typeof(ImageSegment);
    
    public void Build(MessageBuilder builder, ImageSegment segment)
    {
        throw new NotImplementedException();
    }

    public ImageSegment Parse(BotMessage message, ImageEntity entity)
    {
        return new ImageSegment(entity.FileUrl, entity.FileName, entity.Summary, entity.SubType);
    }
}