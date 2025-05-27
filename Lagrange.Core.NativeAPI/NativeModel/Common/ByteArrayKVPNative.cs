using System.Runtime.InteropServices;
// ReSharper disable InconsistentNaming

namespace Lagrange.Core.NativeAPI.NativeModel.Common;

[StructLayout(LayoutKind.Sequential)]
public struct ByteArrayKVPNative
{
    public ByteArrayNative Key;
    public ByteArrayNative Value;
}