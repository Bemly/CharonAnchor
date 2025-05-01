using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Message.Entities;

public class TextEntity(string text) : IMessageEntity
{
    public string Text { get; } = text;

    public TextEntity() : this(string.Empty) { }
    
    Elem[] IMessageEntity.Build()
    {
        throw new NotImplementedException();
    }
    
    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        return target.Text is { Attr6Buf: null } or { Attr6Buf.Length: 0 }
            ? new TextEntity(target.Text.TextMsg) 
            : null;
    }
}