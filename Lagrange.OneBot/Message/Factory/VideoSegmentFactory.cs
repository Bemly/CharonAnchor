using Lagrange.Codec;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.OneBot.Message.Entity;

namespace Lagrange.OneBot.Message.Factory;

public class VideoSegmentFactory : ISegmentFactory<VideoSegment, VideoEntity>
{
    public Type SegmentType => typeof(VideoSegment);
    
    public void Build(MessageBuilder builder, VideoSegment segment)
    {
        if (CommonResolver.ResolveStream(segment.File) is { } stream)
        {
            var array = GC.AllocateUninitializedArray<byte>((int)stream.Length);
            var thumbnail = new MemoryStream(VideoCodec.FirstFrame(array));
            builder.Video(stream, thumbnail);
        }
    }

    public VideoSegment Parse(BotMessage message, VideoEntity entity)
    {
        return new VideoSegment(entity.FileUrl);
    }
}