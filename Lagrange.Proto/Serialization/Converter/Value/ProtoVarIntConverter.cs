using System.Numerics;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

internal class ProtoVarIntConverter<T> : ProtoConverter<T> where T : unmanaged, INumber<T>
{
    public override WireType WireType => WireType.VarInt;
    
    public override void Write(int field, ProtoWriter writer, T value)
    {
        writer.EncodeVarInt<T>(value);
    }

    public override T Read(int field, ref ProtoReader reader)
    {
        return reader.DecodeVarInt<T>();
    }
}