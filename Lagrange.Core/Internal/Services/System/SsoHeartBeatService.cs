using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.System;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<SsoHeartBeatEventReq>(Protocols.PC)]
[Service("trpc.qq_new_tech.status_svc.StatusService.SsoHeartBeat")]
internal class SsoHeartBeatService : BaseService<SsoHeartBeatEventReq, SsoHeartBeatEventResp>
{
    protected override ValueTask<ReadOnlyMemory<byte>> Build(SsoHeartBeatEventReq input, BotContext context)
    {
        var packet = new SsoHeartBeatRequest { Type = 1 };
        
        return new ValueTask<ReadOnlyMemory<byte>>(ProtoHelper.Serialize(packet));
    }

    protected override ValueTask<SsoHeartBeatEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var packet = ProtoHelper.Deserialize<SsoHeartBeatResponse>(input.Span);

        return new ValueTask<SsoHeartBeatEventResp?>(new SsoHeartBeatEventResp((int)packet.Interval));
    }
}