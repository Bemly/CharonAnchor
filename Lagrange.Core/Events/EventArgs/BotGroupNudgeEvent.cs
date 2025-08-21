namespace Lagrange.Core.Events.EventArgs;

public class BotGroupNudgeEvent(long groupUin, long operatorUin, string action, long targetUin, string suffix) : EventBase
{
    public long GroupUin { get; } = groupUin;

    public long OperatorUin { get; } = operatorUin;

    public string Action { get; } = action;

    public long TargetUin { get; } = targetUin;

    public string Suffix { get; } = suffix;

    public override string ToEventMessage()
    {
        return $"{nameof(BotGroupNudgeEvent)}: In Group {GroupUin}, {OperatorUin} {Action} {TargetUin} {Suffix}";
    }
}
