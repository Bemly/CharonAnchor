using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GroupFileSendEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d6_4")]
internal class GroupFileSendService : OidbService<GroupFileSendEventReq, GroupFileSendEventResp, FeedsReqBody, FeedsRspBody>
{
    private protected override uint Command => 0x6d6;

    private protected override uint Service => 4;
    
    private protected override Task<FeedsReqBody> ProcessRequest(GroupFileSendEventReq request, BotContext context)
    {
        return Task.FromResult(new FeedsReqBody
        {
            GroupCode = (ulong)request.GroupUin,
            AppId = 2,
            FeedsInfoList =
            [
                new FeedsInfo
                {
                    BusId = 102,
                    FileId = request.FileId,
                    MsgRandom = request.Random,
                    FeedFlag = 1
                }
            ]
        });
    }

    private protected override Task<GroupFileSendEventResp> ProcessResponse(FeedsRspBody response, BotContext context)
    {
        return Task.FromResult(new GroupFileSendEventResp(response.RetCode, response.RetMsg));
    }
}