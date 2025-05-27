using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Message.Entities;

public class ReplyEntity : IMessageEntity
{
    public ulong SrcUid { get; set; }
    
    public int SrcSequence { get; set; }
    
    internal List<Elem> Elems { get; set; } = new();
    
    public ReplyEntity(BotMessage source)
    {
    }

    public ReplyEntity() { }

    public Task Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    Elem[] IMessageEntity.Build()
    {
        throw new NotImplementedException();
    }

    IMessageEntity? IMessageEntity.Parse(List<Elem> elements, Elem target)
    {
        if (target.SrcMsg is { } srcMsg)
        {
            var resvAttr = ProtoHelper.Deserialize<SourceMsgResvAttr>(srcMsg.PbReserve.Span);
            
            return new ReplyEntity
            {
                SrcUid = resvAttr.SourceMsgId, 
                SrcSequence = (int)srcMsg.OrigSeqs[0],
                Elems = srcMsg.Elems.Select(x => ProtoHelper.Deserialize<Elem>(x.Span)).ToList()
            };
        }
        
        return null;
    }
}