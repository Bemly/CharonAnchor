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

    public MessageBuilder Image(string path)
    {
        _entities.Add(new ImageEntity(new FileStream(path, FileMode.Open, FileAccess.Read)));
        return this;
    }

    public MessageBuilder Image(byte[] image)
    {
        _entities.Add(new ImageEntity(new MemoryStream(image)));
        return this;
    }

    public MessageBuilder Record(string path)
    {
        _entities.Add(new RecordEntity(new FileStream(path, FileMode.Open, FileAccess.Read)));
        return this;
    }

    public MessageBuilder Record(byte[] record)
    {
        _entities.Add(new RecordEntity(new MemoryStream(record)));
        return this;
    }

    public MessageBuilder Video(string path, string? thumbnail)
    {
        var thumbnailStream = string.IsNullOrEmpty(thumbnail) ? null : new FileStream(thumbnail, FileMode.Open, FileAccess.Read);
        _entities.Add(new VideoEntity(new FileStream(path, FileMode.Open, FileAccess.Read), thumbnailStream));
        return this;
    }

    public MessageBuilder Video(byte[] video, byte[]? thumbnail)
    {
        var thumbnailStream = thumbnail == null ? null : new MemoryStream(thumbnail);
        _entities.Add(new VideoEntity(new MemoryStream(video), thumbnailStream));
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