using Lagrange.Core.Internal.Packets.Message;

namespace Lagrange.Core.Internal.Events.Message;

internal class GetC2CMessageEventReq(string peerUid, uint startSequence, uint endSequence) : ProtocolEvent
{
    public string PeerUid { get; } = peerUid;

    public uint StartSequence { get; } = startSequence;

    public uint EndSequence { get; } = endSequence;
}

internal class GetC2CMessageEventResp(List<CommonMessage> chains) : ProtocolEvent
{
    public List<CommonMessage> Chains { get; } = chains;
}