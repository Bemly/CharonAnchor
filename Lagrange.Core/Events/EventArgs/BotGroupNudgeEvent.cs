namespace Lagrange.Core.Events.EventArgs;

public class BotGroupNudgeEvent(Int64 groupUin, Int64 operatorUin, Int64 targetUin) : EventBase
{
    public Int64 GroupUin { get; } = groupUin;

    public Int64 OperatorUin { get; } = operatorUin;

    public Int64 TargetUin { get; } = targetUin;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupNudgeEvent)}: GroupUin={GroupUin}, OperatorUin={OperatorUin}, TargetUin={TargetUin}";
    }
}
