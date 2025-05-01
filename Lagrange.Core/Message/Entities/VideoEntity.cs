using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Message.Entities;

public class VideoEntity : RichMediaEntityBase
{
    internal override Lazy<Stream>? Stream { get; }
    
    public override Task Preprocess(BotContext context, BotMessage message)
    {
        throw new NotImplementedException();
    }

    public override async Task Postprocess(BotContext context, BotMessage message)
    {
        NTV2RichMediaDownloadEventResp? result = message.IsGroup()
            ? await context.EventContext.SendEvent<VideoGroupDownloadEventResp>(new VideoGroupDownloadEventReq(message, this))
            : await context.EventContext.SendEvent<VideoDownloadEventResp>(new VideoDownloadEventReq(message, this));

        if (result == null) return;

        FileUrl = result.Url;
    }
    
    internal override Elem[] Build()
    {
        throw new NotImplementedException();
    }

    internal override IMessageEntity? Parse(List<Elem> elements, Elem target)
    {
        if (target.CommonElem is { BusinessType: 11 or 21 } commonElem)
        {
            return new VideoEntity { MsgInfo = ProtoHelper.Deserialize<MsgInfo>(commonElem.PbElem) };
        }
        return null;
    }
}