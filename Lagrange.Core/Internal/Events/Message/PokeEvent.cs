namespace Lagrange.Core.Internal.Events.Message;

internal class PokeEventReq(bool isGroup, long peerUin, long targetUin) : ProtocolEvent
{
    public bool IsGroup { get; } = isGroup;
    
    public long PeerUin { get; } = peerUin;

    public long TargetUin { get; } = targetUin;
}

internal class PokeEventResp : ProtocolEvent;