using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization.Converter.Generic;

internal class ProtoNullableConverter<T> : ProtoConverter<T?> where T : struct
{
    private readonly ProtoConverter<T> _converter = ProtoTypeResolver.GetConverter<T>();

    public override WireType WireType => _converter.WireType;
    
    public override void Write(int field, ProtoWriter writer, T? value)
    {
        if (value.HasValue) _converter.Write(0, writer, value.Value);
    }

    public override T? Read(int field, ref ProtoReader reader)
    {
        return _converter.Read(field, ref reader);
    }
}