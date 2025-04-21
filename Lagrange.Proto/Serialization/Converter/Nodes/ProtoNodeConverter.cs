using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoNodeConverter : ProtoConverter<ProtoNode>
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, ProtoNode value)
    {
        throw new NotImplementedException();
    }

    public override int Measure(int field, WireType wireType, ProtoNode value)
    {
        throw new NotImplementedException();
    }

    public override ProtoNode Read(int field, WireType wireType, ref ProtoReader reader)
    {
        throw new NotImplementedException();
    }
}