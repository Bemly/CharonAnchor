using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Lagrange.Proto;

[StackTraceHidden]
internal static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_NeedLargerSpan() => throw new InvalidOperationException("Need larger span");
    
    [DoesNotReturn]
    public static void ThrowInvalidDataException_MalformedMessage() => throw new InvalidDataException("Malformed proto message while decoding");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowInvalidOperationException_FailedDetermineConverter<T>() => throw new InvalidOperationException($"Unable to determine the type of the object to serialize for {typeof(T).Name}");

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowObjectDisposedException_ProtoWriter() => throw new ObjectDisposedException("ProtoWriter", "The ProtoWriter has been disposed. Please ensure that the ProtoWriter is not used after it has been disposed.");
    
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentOutOfRangeException_NoEnoughSpace(string type, int size, int available) => throw new ArgumentOutOfRangeException(type, $"The {type} size is {size}, but only {available} bytes are available.");
}