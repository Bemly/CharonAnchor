using System.Numerics;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Serialization.Converter;

internal class ProtoNumberConverter<T> : ProtoConverter<T> where T : unmanaged, INumber<T>
{
    public override bool ShouldSerialize(T value, bool ignoreDefaultValue)
    {
        return !ignoreDefaultValue && value != default;
    }
    
    public override void Write(int field, WireType wireType, ProtoWriter writer, T value)
    {
        switch (wireType)
        {
            case WireType.Fixed32:
                writer.EncodeFixed32(value);
                break;
            case WireType.Fixed64:
                writer.EncodeFixed64(value);
                break;
            case WireType.VarInt:
                writer.EncodeVarInt(value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wireType), wireType, null);
        }
    }

    public override void WriteWithNumberHandling(int field, WireType wireType, ProtoWriter writer, T value, ProtoNumberHandling numberHandling)
    {
        if ((numberHandling & ProtoNumberHandling.Signed) != 0)
        {
            Write(field, wireType, writer, ProtoHelper.ZigZagEncode(value));
        }
        else
        {
            Write(field, wireType, writer, value);
        }
    }

    public override int Measure(int field, WireType wireType, T value)
    {
        return wireType switch
        {
            WireType.Fixed32 => sizeof(float),
            WireType.Fixed64 => sizeof(double),
            WireType.VarInt => ProtoHelper.GetVarIntLength(value),
            _ => throw new ArgumentOutOfRangeException(nameof(wireType), wireType, null)
        };
    }

    public override T Read(int field, WireType wireType, ref ProtoReader reader)
    {
        return wireType switch
        {
            WireType.Fixed32 => reader.DecodeFixed32<T>(),
            WireType.Fixed64 => reader.DecodeFixed64<T>(),
            WireType.VarInt => reader.DecodeVarInt<T>(),
            _ => throw new ArgumentOutOfRangeException(nameof(wireType), wireType, null)
        };
    }
    
    public override T ReadWithNumberHandling(int field, WireType wireType, ref ProtoReader reader, ProtoNumberHandling numberHandling)
    {
        T value = Read(field, wireType, ref reader);
        return (numberHandling & ProtoNumberHandling.Signed) != 0 ? ProtoHelper.ZigZagDecode(value) : value;
    }
}