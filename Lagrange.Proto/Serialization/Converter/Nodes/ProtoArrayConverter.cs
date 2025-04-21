using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoArrayConverter : ProtoConverter<ProtoArray>
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, ProtoArray value)
    {
        throw new NotImplementedException();
    }

    public override int Measure(int field, WireType wireType, ProtoArray value)
    {
        throw new NotImplementedException();
    }

    public override ProtoArray Read(int field, WireType wireType, ref ProtoReader reader)
    {
        throw new NotImplementedException();
    }
}