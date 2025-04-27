namespace Lagrange.Core.Internal.Events.Login;

internal class QrLoginEventReq(byte[] k, bool? isApproved) : ProtocolEvent
{
    public byte[] K { get; } = k;

    public bool? IsApproved { get; } = isApproved;
}

internal class QrLoginEventResp : ProtocolEvent
{
    
}