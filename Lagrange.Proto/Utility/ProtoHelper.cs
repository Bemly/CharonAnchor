using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lagrange.Proto.Utility;

public static class ProtoHelper
{
    /// <summary>
    /// This function should only be used when writing the string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (int, int) GetVarIntRange(int length) => length switch
    {
        1 => (1 << 0, (1 << 7) - 1),
        2 => (1 << 7, (1 << 14) - 1),
        3 => (1 << 14, (1 << 21) - 1),
        4 => (1 << 21, (1 << 28) - 1),
        _ => throw new ArgumentOutOfRangeException(nameof(length), "Invalid length for VarInt.")
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetVarIntLength<T>(T value) where T : unmanaged, INumberBase<T>
    {
        ulong v = ulong.CreateTruncating(value);
        return BitOperations.TrailingZeroCount(v) / 7 + 1;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T ZigZagEncode<T>(T value) 
        where T : unmanaged, INumber<T>, IShiftOperators<T, int, T>, IBitwiseOperators<T, T, T>
    {
        return (value << 1) ^ (value >> (sizeof(T) * 8 - 1));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ZigZagDecode<T>(T value) 
        where T : unmanaged, INumber<T>, IShiftOperators<T, int, T>, IBitwiseOperators<T, T, T>
    {
        return (value >> 1) ^ -(value & T.One);
    }
}