using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoObjectConverter : ProtoConverter<ProtoObject>
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, ProtoObject value)
    {
        throw new NotImplementedException();
    }

    public override int Measure(int field, WireType wireType, ProtoObject value)
    {
        throw new NotImplementedException();
    }

    public override ProtoObject Read(int field, WireType wireType, ref ProtoReader reader)
    {
        throw new NotImplementedException();
    }
}