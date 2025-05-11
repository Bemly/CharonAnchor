using Lagrange.Core.NativeAPI.ReverseEvent;

namespace Lagrange.Core.NativeAPI
{
    public class Context
    {
        public Context(BotContext botContext)
        {
            BotContext = botContext;
            EventInvoker = new ReverseEventInvoker(BotContext);
        }
        public BotContext BotContext { get; set; }
        public ReverseEventInvoker EventInvoker { get; set; }
    }
}