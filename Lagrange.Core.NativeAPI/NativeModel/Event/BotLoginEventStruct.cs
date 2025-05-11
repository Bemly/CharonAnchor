using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public class BotLoginEventStruct : IEventStruct
    {
        public int State = 0;
        public byte[] Tag = [];
        public byte[] Message = [];
    }
}