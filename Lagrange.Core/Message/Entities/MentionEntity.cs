using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class MentionEntity(long uin, string display) : IMessageEntity
{
    public long Uin { get; } = uin;

    public string Display { get; } = display;
    
    internal string? Uid { get; private set; }
    
    public MentionEntity() : this(0, string.Empty) { }
    
    Task IMessageEntity.Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    Task IMessageEntity.Postprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    Elem[] IMessageEntity.Build()
    {
        throw new NotImplementedException();
    }
    
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        return null;
    }
}
