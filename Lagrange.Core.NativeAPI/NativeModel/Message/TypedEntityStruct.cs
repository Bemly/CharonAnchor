using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Message
{
    [StructLayout(LayoutKind.Sequential)]
    public class TypedEntityStruct
    {
        //需要手动释放
        public IntPtr Entity = IntPtr.Zero;
        public int Type = 0;
    }
}