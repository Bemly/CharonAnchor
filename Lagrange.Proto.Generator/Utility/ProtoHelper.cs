using Lagrange.Proto.Generator.Utility.Extension;
using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator.Utility;

public static class ProtoHelper
{
    public static byte[] EncodeVarInt(int value)
    {
        Span<byte> result = stackalloc byte[5];
        int i = 0;
        while (value > 127)
        {
            result[i] = (byte)((value & 0x7F) | 0x80);
            value >>= 7;
            i++;
        }
        result[i] = ((byte)value);
        
        return result.Slice(0, i + 1).ToArray();
    }

    public static WireType GetWireType(this TypeSyntax type)
    {
        if (type.IsNumberType()) return WireType.VarInt;
        if (type.IsSingleType()) return WireType.Fixed32;
        if (type.IsDoubleType()) return WireType.Fixed64;

        return WireType.LengthDelimited;
    }
}