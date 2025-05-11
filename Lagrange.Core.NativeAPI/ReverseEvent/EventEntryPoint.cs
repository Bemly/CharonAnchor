using System.Runtime.InteropServices;
using Lagrange.Core.Events;
using Lagrange.Core.NativeAPI.NativeModel.Event;
using Lagrange.Core.NativeAPI.ReverseEvent.Abstract;

namespace Lagrange.Core.NativeAPI.ReverseEvent
{
    public static class EventEntryPoint
    {
        [UnmanagedCallersOnly(EntryPoint = "GetLoginEvent")]
        public static IntPtr GetLoginEvent(int index)
        {
            if (index >= Program.Contexts.Count)
            {
                return IntPtr.Zero;
            }
            
            var botLoginEvent = Program.Contexts[index].EventInvoker.BotLoginEvent;
            
            IntPtr eventPtr = GetEventStructPtr<BotLoginEventStruct>(botLoginEvent);
            
            return eventPtr;
        }
        
        [UnmanagedCallersOnly(EntryPoint = "GetBotLogEvent")]
        public static IntPtr GetBotLogEvent(int index)
        {
            if (index >= Program.Contexts.Count)
            {
                return IntPtr.Zero;
            }
            
            var botLogEvent = Program.Contexts[index].EventInvoker.BotLogEvent;
            
            IntPtr eventPtr = GetEventStructPtr<BotLogEventStruct>(botLogEvent);
            
            return eventPtr;
        }
        
        [UnmanagedCallersOnly(EntryPoint = "GetCaptchaEvent")]
        public static IntPtr GetCaptchaEvent(int index)
        {
            if (index >= Program.Contexts.Count)
            {
                return IntPtr.Zero;
            }
            
            var botCaptchaEvent = Program.Contexts[index].EventInvoker.BotCaptchaEvent;
            
            IntPtr eventPtr = GetEventStructPtr<BotCaptchaEventStruct>(botCaptchaEvent);
            
            return eventPtr;
        }

        private static IntPtr GetEventStructPtr<T>(ReverseEventBase reverseEvent) where T : IEventStruct
        {
            if (reverseEvent.Events.Count == 0)
            {
                return IntPtr.Zero;
            }
            
            IntPtr eventPtr = Marshal.AllocHGlobal(reverseEvent.Events.Count * Marshal.SizeOf<T>());
            for (int i = 0; i < reverseEvent.Events.Count; i++)
            {
                Marshal.StructureToPtr(
                    reverseEvent.Events[i],
                    eventPtr + i * Marshal.SizeOf<T>(),
                    false
                );
            }

            reverseEvent.Events.Clear();
            return eventPtr;
        }
    }
}
