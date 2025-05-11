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
        if (CommonResolver.ResolveStream(segment.File) is { } stream)
        {
            builder.Image(stream, segment.Summary, segment.SubType);
        }
    }

    public ImageSegment Parse(BotMessage message, ImageEntity entity)
    {
        return new ImageSegment(entity.FileUrl, entity.FileName, entity.Summary, entity.SubType);
    }
}