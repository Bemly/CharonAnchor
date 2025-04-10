using System.Numerics;
using System.Runtime.CompilerServices;

namespace Lagrange.Proto.Utility;

internal static class ProtoHelper
{
    /// <summary>
    /// This function should only be used when writing the string.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) GetVarIntRange(int length) => length switch
    {
        1 => (1 << 0, (1 << 7) - 1),
        2 => (1 << 7, (1 << 14) - 1),
        3 => (1 << 14, (1 << 21) - 1),
        4 => (1 << 21, (1 << 28) - 1),
        _ => throw new ArgumentOutOfRangeException(nameof(length), "Invalid length for VarInt.")
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetVarIntLength<T>(T value) where T : unmanaged, INumberBase<T>
    {
        ulong v = ulong.CreateTruncating(value);
        return BitOperations.LeadingZeroCount(v) / 7 + 1;
    }
}