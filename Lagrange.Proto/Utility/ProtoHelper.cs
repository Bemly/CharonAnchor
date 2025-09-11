using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Utility;

public static class ProtoHelper
{
    private static readonly int[] VarIntValues;

    static ProtoHelper()
    {
        VarIntValues = new int[5];
        for (int i = 0; i < VarIntValues.Length; i++) VarIntValues[i] = 1 << (7 * i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetVarIntMin(int length) => VarIntValues[length - 1];
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetVarIntMax(int length) => VarIntValues[length] - 1;

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
        return sizeof(T) switch
        {
            1 or 2 or 4 => T.CreateTruncating(EncodeZigZag32(int.CreateSaturating(value))),
            8 => T.CreateTruncating(EncodeZigZag64(long.CreateSaturating(value))),
            _ => sizeof(T) <= 4
                ? T.CreateTruncating(EncodeZigZag32(int.CreateSaturating(value)))
                : T.CreateTruncating(EncodeZigZag64(long.CreateSaturating(value)))
        };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint EncodeZigZag32(int n) => (uint)((n << 1) ^ (n >> 31));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong EncodeZigZag64(long n) => (ulong)((n << 1) ^ (n >> 63));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T ZigZagDecode<T>(T value) where T : unmanaged, INumber<T>
    {
        return sizeof(T) switch
        {
            1 => T.CreateTruncating(DecodeZigZag32(uint.CreateTruncating(value) & 0xFF)),
            2 => T.CreateTruncating(DecodeZigZag32(uint.CreateTruncating(value) & 0xFFFF)),
            4 => T.CreateTruncating(DecodeZigZag32(uint.CreateTruncating(value))),
            8 => T.CreateTruncating(DecodeZigZag64(ulong.CreateTruncating(value))),
            _ => sizeof(T) <= 4
                ? T.CreateTruncating(DecodeZigZag32(uint.CreateTruncating(value)))
                : T.CreateTruncating(DecodeZigZag64(ulong.CreateTruncating(value)))
        };
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int DecodeZigZag32(uint n) => (int)((n >> 1) ^ (uint)(-(int)(n & 1)));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static long DecodeZigZag64(ulong n) => (long)((n >> 1) ^ (ulong)(-(long)(n & 1)));
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