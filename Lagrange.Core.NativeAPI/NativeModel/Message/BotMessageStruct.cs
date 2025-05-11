using System.Runtime.InteropServices;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.NativeAPI.NativeModel.Message.Entity;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BotMessageStruct
    {
        public BotMessageStruct() { }
        
        IntPtr Contact = IntPtr.Zero;
        
        BotGroupStruct BotGroup = new();
        
        public int Type = 0;
        
        public byte[] Time = [];
        
        public TypedEntityStruct[] Entities = [];
    }
}