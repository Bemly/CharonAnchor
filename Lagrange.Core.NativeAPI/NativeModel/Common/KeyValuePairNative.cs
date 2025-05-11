using System.Runtime.InteropServices;

namespace Lagrange.Core.NativeAPI.NativeModel.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct KeyValuePairNative<T1, T2>
    {
        public T1 Key;
        public T2 Value;
    }
}