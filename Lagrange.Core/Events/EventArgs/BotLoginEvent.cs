namespace Lagrange.Core.Events.EventArgs;

public class BotLoginEvent(byte state, (string, string)? error) : EventBase
{
    public bool Success => State == 0;
    
    public byte State { get; } = state;

    public (string Tag, string Message)? Error { get; } = error;

    public override string ToEventMessage() => Error == null
        ? $"[{nameof(BotLoginEvent)}] State: {State}"
        : $"[{nameof(BotLoginEvent)}] State: {State} | Error: {Error?.Tag} | Message: {Error?.Message}";
}