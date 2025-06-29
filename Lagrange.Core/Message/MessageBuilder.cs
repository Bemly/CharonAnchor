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

    public MessageBuilder Mention(long uin, string? display)
    {
        _entities.Add(new MentionEntity(uin, display));
        return this;
    }

    public MessageBuilder Reply(BotMessage source)
    {
        _entities.Add(new ReplyEntity(source));
        return this;
    }

    public MessageBuilder MultiMsg(List<BotMessage> messages)
    {
        _entities.Add(new MultiMsgEntity(messages));
        return this;
    }
    
    public MessageBuilder MultiMsg(string resId)
    {
        _entities.Add(new MultiMsgEntity(resId));
        return this;
    }

    public MessageBuilder Image(string path, string? summary = "[图片]", int subType = 0)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Image(fs, summary, subType);
    }

    public MessageBuilder Image(byte[] image, string? summary = "[图片]", int subType = 0)
    {
        using var ms = new MemoryStream(image);
        return Image(ms, summary, subType);
    }

    public MessageBuilder Image(Stream stream, string? summary = "[图片]", int subType = 0)
    {
        _entities.Add(new ImageEntity(stream, summary, subType));
        return this;
    }

    public MessageBuilder Record(string path)
    {
        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        return Record(fs);
    }

    public MessageBuilder Record(byte[] record)
    {
        using var ms = new MemoryStream(record);
        return Record(ms);
    }

    public MessageBuilder Record(Stream stream)
    {
        _entities.Add(new RecordEntity(stream));
        return this;
    }

    public MessageBuilder Video(string path, string? thumbnail)
    {
        using var videoFs = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var thumbnailFs = thumbnail != null ? new FileStream(thumbnail, FileMode.Open, FileAccess.Read) : null;
        return Video(videoFs, thumbnailFs);
    }

    public MessageBuilder Video(byte[] video, byte[]? thumbnail)
    {
        using var videoStream = new MemoryStream(video);
        using var thumbnailStream = thumbnail != null ? new MemoryStream(thumbnail) : null;
        return Video(videoStream, thumbnailStream);
    }

    public MessageBuilder Video(Stream video, Stream? thumbnail)
    {
        _entities.Add(new VideoEntity(video, thumbnail));
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