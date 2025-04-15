using System.Runtime.CompilerServices;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public unsafe class ProtoEnumConverter<T> : ProtoConverter<T> where T : unmanaged, Enum
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, T value)
    {
        switch (sizeof(T))
        {
            case sizeof(byte):
            {
                switch (wireType)
                {
                    case WireType.Fixed32: writer.EncodeFixed32(Unsafe.As<T, byte>(ref value)); break;
                    case WireType.Fixed64: writer.EncodeFixed64(Unsafe.As<T, byte>(ref value)); break;
                    case WireType.VarInt: writer.EncodeVarInt(Unsafe.As<T, byte>(ref value)); break;
                }
                break;
            }
            case sizeof(short):
            {
                switch (wireType)
                {
                    case WireType.Fixed32: writer.EncodeFixed32(Unsafe.As<T, short>(ref value)); break;
                    case WireType.Fixed64: writer.EncodeFixed64(Unsafe.As<T, short>(ref value)); break;
                    case WireType.VarInt: writer.EncodeVarInt(Unsafe.As<T, short>(ref value)); break;
                }
                break;
            }
            case sizeof(int):
            {
                switch (wireType)
                {
                    case WireType.Fixed32: writer.EncodeFixed32(Unsafe.As<T, int>(ref value)); break;
                    case WireType.Fixed64: writer.EncodeFixed64(Unsafe.As<T, int>(ref value)); break;
                    case WireType.VarInt: writer.EncodeVarInt(Unsafe.As<T, int>(ref value)); break;
                }
                break;
            }
            case sizeof(long):
            {
                switch (wireType)
                {
                    case WireType.Fixed32: writer.EncodeFixed32(Unsafe.As<T, long>(ref value)); break;
                    case WireType.Fixed64: writer.EncodeFixed64(Unsafe.As<T, long>(ref value)); break;
                    case WireType.VarInt: writer.EncodeVarInt(Unsafe.As<T, long>(ref value)); break;
                }
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(nameof(wireType), wireType, null);
            }
        }
    }

    public override T Read(int field, WireType wireType, ref ProtoReader reader)
    {
        long value = wireType switch
        {
            WireType.Fixed32 => reader.DecodeFixed32<int>(),
            WireType.Fixed64 => reader.DecodeFixed64<long>(),
            WireType.VarInt => reader.DecodeVarInt<long>(),
            _ => throw new ArgumentOutOfRangeException(nameof(wireType), wireType, null)
        };
        
        return Unsafe.As<long, T>(ref value);
    }
}