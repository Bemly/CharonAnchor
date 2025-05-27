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

    public MessageBuilder Image(string path, string? summary = "[图片]", int subType = 0) =>
        Image(new FileStream(path, FileMode.Open, FileAccess.Read), summary, subType);

    public MessageBuilder Image(byte[] image, string? summary = "[图片]", int subType = 0) =>
        Image(new MemoryStream(image), summary, subType);
    
    public MessageBuilder Image(Stream stream, string? summary = "[图片]", int subType = 0)
    {
        _entities.Add(new ImageEntity(stream, summary, subType));
        return this;
    }

    public MessageBuilder Record(string path) => Record(new FileStream(path, FileMode.Open, FileAccess.Read));

    public MessageBuilder Record(byte[] record) => Record(new MemoryStream(record));
    
    public MessageBuilder Record(Stream stream)
    {
        _entities.Add(new RecordEntity(stream));
        return this;
    }

    public MessageBuilder Video(string path, string? thumbnail) =>
        Video(new FileStream(path, FileMode.Open, FileAccess.Read), thumbnail != null ? new FileStream(thumbnail, FileMode.Open, FileAccess.Read) : null);

    public MessageBuilder Video(byte[] video, byte[]? thumbnail) =>
        Video(new MemoryStream(video), thumbnail != null ? new MemoryStream(thumbnail) : null);
    
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