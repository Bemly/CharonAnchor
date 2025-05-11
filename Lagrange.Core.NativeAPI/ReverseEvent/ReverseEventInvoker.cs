namespace Lagrange.Core.NativeAPI.ReverseEvent
{
    public class ReverseEventInvoker
    {
        public ReverseEventInvoker(BotContext context)
        {
            BotCaptchaEvent.RegisterEventHandler(context);
            BotLoginEvent.RegisterEventHandler(context);
            BotLogEvent.RegisterEventHandler(context);
        }
        
        public BotCaptchaReverseEvent BotCaptchaEvent { get; } = new();
        
        public BotLoginReverseEvent BotLoginEvent { get; } = new();
        
        public BotLogReverseEvent BotLogEvent { get; } = new();
    }
}