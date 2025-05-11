using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Event
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotLogEventStruct : IEventStruct
    {
        public BotLogEventStruct() { }

        public int Level = 0;
        public byte[] Tag = [];
        public byte[] Message = [];
    }
}
