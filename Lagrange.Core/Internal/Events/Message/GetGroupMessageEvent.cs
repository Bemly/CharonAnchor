using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Internal.Events.Message;

internal class GetGroupMessageEventReq(long groupUin, int startSequence, int endSequence) : ProtocolEvent
{
    public long GroupUin { get; } = groupUin;

    public int StartSequence { get; } = startSequence;

    public int EndSequence { get; } = endSequence;
}

internal class GetGroupMessageEventResp(List<CommonMessage> chains) : ProtocolEvent
{
    public List<CommonMessage> Chains { get; } = chains;
}