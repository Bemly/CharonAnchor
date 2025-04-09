namespace Lagrange.Core.Events.EventArgs;

public class BotQrCodeQueryEvent(BotQrCodeQueryEvent.TransEmpState state) : EventBase
{
    public TransEmpState State { get; } = state;
    
    public override string ToEventMessage() => $"[{nameof(BotQrCodeQueryEvent)}] State: {State}";
    
    public enum TransEmpState : byte
    {
        Confirmed = 0,
        CodeExpired = 17,
        WaitingForScan = 48,
        WaitingForConfirm = 53,
        Canceled = 54,
        Invalid = 144
    }
}