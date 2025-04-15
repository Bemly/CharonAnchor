using System.Runtime.CompilerServices;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public unsafe class ProtoEnumConverter<T> : ProtoConverter<T> where T : unmanaged, Enum
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, T value)
    {
        switch (sizeof(T))
        {
            case sizeof(byte): writer.EncodeVarInt(Unsafe.As<T, byte>(ref value)); break;
            case sizeof(short): writer.EncodeVarInt(Unsafe.As<T, short>(ref value)); break;
            case sizeof(int): writer.EncodeVarInt(Unsafe.As<T, int>(ref value)); break;
            case sizeof(long): writer.EncodeVarInt(Unsafe.As<T, long>(ref value)); break;
            default: throw new ArgumentOutOfRangeException(nameof(wireType), wireType, null);
        }
    }

    public override T Read(int field, WireType wireType, ref ProtoReader reader)
    {
        long value = reader.DecodeVarInt<long>();
        return Unsafe.As<long, T>(ref value);
    }
}