using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Lagrange.Proto;

[StackTraceHidden]
internal class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowInvalidOperationException_NeedLargerSpan() => throw new InvalidOperationException("Need larger span");

    [DoesNotReturn]
    public static void ThrowObjectDisposedException_ProtoWriter() => throw new ObjectDisposedException("ProtoWriter", "The ProtoWriter has been disposed. Please ensure that the ProtoWriter is not used after it has been disposed.");
}