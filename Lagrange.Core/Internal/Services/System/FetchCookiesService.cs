using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Service;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<FetchCookiesEventReq>(Protocols.All)]
[Service("OidbSvcTrpcTcp.0x102a_0")]
internal class FetchCookiesService : OidbService<FetchCookiesEventReq, FetchCookiesEventResp, C102AReqBody, C102ARspBody>
{
    private protected override uint Command => 0x102A;

    private protected override uint Service => 0;
    
    private protected override Task<C102AReqBody> ProcessRequest(FetchCookiesEventReq request, BotContext context)
    {
        return Task.FromResult(new C102AReqBody { Domain = request.Domain });
    }

    private protected override Task<FetchCookiesEventResp> ProcessResponse(C102ARspBody response, BotContext context)
    {
        return Task.FromResult(new FetchCookiesEventResp(response.PsKeys));
    }
}