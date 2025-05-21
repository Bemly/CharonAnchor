using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.Message;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.Message;

[EventSubscribe<GroupFileSendEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x6d9_4")]
internal class GroupFileSendService : OidbService<GroupFileSendEventReq, GroupFileSendEventResp, D6D9ReqBody, D6D9RspBody>
{
    private protected override uint Command => 0x6d9;

    private protected override uint Service => 4;
    
    private protected override Task<D6D9ReqBody> ProcessRequest(GroupFileSendEventReq request, BotContext context)
    {
        return Task.FromResult(new D6D9ReqBody
        {
            FeedsInfoReq = new FeedsReqBody
            {
                GroupCode = (ulong)request.GroupUin,
                AppId = 2,
                FeedsInfoList = [new FeedsInfo { BusId = 102, FileId = request.FileId, MsgRandom = request.Random, FeedFlag = 1 }]
            }
        });
    }

    private protected override Task<GroupFileSendEventResp> ProcessResponse(D6D9RspBody response, BotContext context)
    {
        return Task.FromResult(new GroupFileSendEventResp(response.FeedsInfoRsp.RetCode, response.FeedsInfoRsp.RetMsg));
    }
}