using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.System;
using Lagrange.Core.Utility;
using Lagrange.Proto.Nodes;

namespace Lagrange.Core.Internal.Services.System;

[EventSubscribe<InfoSyncPushEvent>(Protocols.All)]
[Service("trpc.msg.register_proxy.RegisterProxy.InfoSyncPush")]
internal class InfoSyncPushService : BaseService<InfoSyncPushEvent, InfoSyncPushEvent>
{
    protected override ValueTask<InfoSyncPushEvent?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        var push = ProtoHelper.Deserialize<InfoSyncPush>(input.Span);

        if (push.PushFlag == 2)
        {
            var obj = ProtoObject.Parse(input.Span);

            var field7 = obj[7].AsObject();
            var field8 = obj[8].AsObject();
        }
        
        return base.Parse(input, context);
    }
}