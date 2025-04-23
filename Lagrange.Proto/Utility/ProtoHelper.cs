using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Utility;

public static class ProtoHelper
{
    /*
     *     private static final int[] VAR_INT_LENGTHS = new int[65];

    static {
        for (int i = 0; i <= 64; ++i) {
            VAR_INT_LENGTHS[i] = (63 - i) / 7;
        }
    }
     */
    
    private static readonly int[] VarIntLengths = new int[65];
    
    static ProtoHelper()
    {
        for (int i = 0; i <= 64; ++i) VarIntLengths[i] = (63 - i) / 7;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetVarIntMin(int length) => length switch
    {
        1 => (1 << 0),
        2 => (1 << 7),
        3 => (1 << 14),
        4 => (1 << 21),
        _ => throw new ArgumentOutOfRangeException(nameof(length), "Invalid length for VarInt.")
    };
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetVarIntMax(int length) => length switch
    {
        1 => (1 << 7) - 1,
        2 => (1 << 14) - 1,
        3 => (1 << 21) - 1,
        4 => (1 << 28) - 1,
        _ => throw new ArgumentOutOfRangeException(nameof(length), "Invalid length for VarInt.")
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int GetVarIntLength<T>(T value) where T : unmanaged, INumberBase<T>
    {
        if (value == T.Zero) return 1;
        
        if (sizeof(T) <= 4)
        {
            int leadingZeros = BitOperations.LeadingZeroCount(uint.CreateSaturating(value));
            return (((38 - leadingZeros) * 0b10010010010010011) >> 19) + (leadingZeros >> 5);
        }
        else
        {
            int leadingZeros = BitOperations.LeadingZeroCount(ulong.CreateSaturating(value));
            return (((70 - leadingZeros) * 0b10010010010010011) >> 19) + (leadingZeros >> 6);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T ZigZagEncode<T>(T value) where T : unmanaged, INumber<T>
    {
        if (sizeof(T) <= 4)
        {
            int v = int.CreateSaturating(value);
            return T.CreateTruncating((v << 1) ^ (v >> 31));
        }
        else
        {
            long v = long.CreateSaturating(value);
            return T.CreateTruncating((v << 1) ^ (v >> 63));
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T ZigZagDecode<T>(T value) where T : unmanaged, INumber<T>
    {
        if (sizeof(T) <= 4)
        {
            int v = int.CreateSaturating(value);
            return T.CreateTruncating((v >> 1) ^ -(v & 1));
        }
        else
        {
            long v = long.CreateSaturating(value);
            return T.CreateTruncating((v >> 1) ^ -(v & 1));
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountString(ReadOnlySpan<char> str)
    {
        int length = Encoding.UTF8.GetByteCount(str);
        return GetVarIntLength(length) + length;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CountBytes(ReadOnlySpan<byte> str)
    {
        return GetVarIntLength(str.Length) + str.Length;
    }
}