using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Internal.Events.Message;

internal class GetC2CMessageEventReq(string peerUid, int startSequence, int endSequence) : ProtocolEvent
{
    public string PeerUid { get; } = peerUid;

    public int StartSequence { get; } = startSequence;

    public int EndSequence { get; } = endSequence;
}

internal class GetC2CMessageEventResp(List<CommonMessage> chains) : ProtocolEvent
{
    public List<CommonMessage> Chains { get; } = chains;
}