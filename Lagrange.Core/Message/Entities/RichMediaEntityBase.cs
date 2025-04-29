using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Message.Entities;

public abstract class RichMediaEntityBase : IMessageEntity
{
    internal MsgInfo? MsgInfo { get; private protected set; }
    
    internal abstract Lazy<Stream>? Stream { get; }
    
    public string Url { get; protected set; } = string.Empty;
    
    public abstract Task Preprocess(BotContext context, BotMessage message);

    public abstract Task Postprocess(BotContext context, BotMessage message);
    
    internal abstract Elem[] Build();
    
    internal abstract IMessageEntity? Parse(List<Elem> elements, Elem target);
    
    Elem[] IMessageEntity.Build() => Build();

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target) => Parse(elements, target);
}