using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Message.Entities;

public class ImageEntity : RichMediaEntityBase
{
    internal override Lazy<Stream>? Stream { get; }
    
    public override Task Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    public override Task Postprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    internal override Elem[] Build()
    {
        throw new NotImplementedException();
    }

    internal override IMessageEntity? Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is { BusinessType: 10 or 20 } commonElem)
        {
            MsgInfo = ProtoHelper.Deserialize<MsgInfo>(commonElem.PbElem);
        }
        
        return null;
    }
}