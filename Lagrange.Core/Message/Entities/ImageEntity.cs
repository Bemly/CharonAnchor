using System.Numerics;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Message.Entities;

public class ImageEntity : RichMediaEntityBase
{
    internal override Lazy<Stream>? Stream { get; }
    
    public Vector2 ImageSize { get; set; }
    
    public int SubType { get; set; }
    
    public string Summary { get; internal set; } = "[图片]";
    
    public override Task Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    public override async Task Postprocess(BotContext context, BotMessage message)
    {
        NTV2RichMediaDownloadEventResp result = message.IsGroup()
            ? await context.EventContext.SendEvent<ImageGroupDownloadEventResp>(new ImageGroupDownloadEventReq(message, this))
            : await context.EventContext.SendEvent<ImageDownloadEventResp>(new ImageDownloadEventReq(message, this));
        
        FileUrl = result.Url;
    }

    internal override Elem[] Build()
    {
        throw new NotImplementedException();
    }

    internal override IMessageEntity? Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is { BusinessType: 10 or 20 } commonElem)
        {
            var msgInfo = ProtoHelper.Deserialize<MsgInfo>(commonElem.PbElem);
            var info = msgInfo.MsgInfoBody[0].Index.Info;
            
            return new ImageEntity
            {
                MsgInfo = msgInfo,
                ImageSize = new Vector2(info.Width, info.Height),
                SubType = (int)msgInfo.ExtBizInfo.Pic.BizType,
                Summary = string.IsNullOrEmpty(msgInfo.ExtBizInfo.Pic.TextSummary) ? "[图片]" : msgInfo.ExtBizInfo.Pic.TextSummary,
            };
        }
        
        return null;
    }
}