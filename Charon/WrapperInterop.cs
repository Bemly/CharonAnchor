using System.Runtime.InteropServices;

namespace CharonAnchor;

/// <summary>
/// P/Invoke bindings for wrapper.node and libdl
/// </summary>
internal static partial class WrapperInterop
{
    // Output buffer layout (matches wrapper.node)
    public const int TokenDataOffset = 0x000;
    public const int TokenLenOffset = 0x0FF;
    public const int ExtraDataOffset = 0x100;
    public const int ExtraLenOffset = 0x1FF;
    public const int SignDataOffset = 0x200;
    public const int SignLenOffset = 0x2FF;
    public const int OutputBufferSize = 0x300;

    // Sign function delegate - matches wrapper.node internal function
    // V1 (old): func(cmd, data, len, seq, output) -> long
    // V2 (new): func(module_id, data, len, seq, output) -> int
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long SignFuncV1Delegate(
        [MarshalAs(UnmanagedType.LPStr)] string cmd,
        IntPtr data,
        int dataLen,
        int seq,
        IntPtr output);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int SignFuncV2Delegate(
        [MarshalAs(UnmanagedType.LPStr)] string moduleId,
        IntPtr data,
        int dataLen,
        int seq,
        IntPtr output);

    // dlopen flags
    public const int RTLD_LAZY = 1;
    public const int RTLD_NOW = 2;
    public const int RTLD_GLOBAL = 0x100;
    public const int RTLD_LOCAL = 0;

    // libdl functions
    [LibraryImport("libdl.so.2")]
    public static partial IntPtr dlopen([MarshalAs(UnmanagedType.LPStr)] string filename, int flags);

    [LibraryImport("libdl.so.2")]
    public static partial int dlclose(IntPtr handle);

    [LibraryImport("libdl.so.2")]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static partial string dlerror();

    // symlink/unlink for bypass
    [LibraryImport("libc.so.6")]
    public static partial int symlink([MarshalAs(UnmanagedType.LPStr)] string target, [MarshalAs(UnmanagedType.LPStr)] string linkpath);

    [LibraryImport("libc.so.6")]
    public static partial int unlink([MarshalAs(UnmanagedType.LPStr)] string pathname);

    [LibraryImport("libc.so.6")]
    public static partial int mkdir([MarshalAs(UnmanagedType.LPStr)] string pathname, uint mode);

    // Get function pointer from address
    public static T GetDelegateForFunctionPointer<T>(nuint address) where T : Delegate
    {
        return Marshal.GetDelegateForFunctionPointer<T>((IntPtr)address);
    }
}