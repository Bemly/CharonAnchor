using System.Numerics;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

internal class ProtoFixed64Converter<T> : ProtoConverter<T> where T : unmanaged, INumber<T>
{
    public override WireType WireType => WireType.Fixed64;
    
    public override void Write(int field, ProtoWriter writer, T value)
    {
        writer.EncodeFixed64<T>(value);
    }

    public override T Read(int field, ref ProtoReader reader)
    {
        return reader.DecodeFixed64<T>();
    }
}