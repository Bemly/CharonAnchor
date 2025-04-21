using Lagrange.Proto.Nodes;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoValueConverter : ProtoConverter<ProtoValue>
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, ProtoValue value)
    {
        throw new NotImplementedException();
    }

    public override int Measure(int field, WireType wireType, ProtoValue value)
    {
        throw new NotImplementedException();
    }

    public override ProtoValue Read(int field, WireType wireType, ref ProtoReader reader)
    {
        throw new NotImplementedException();
    }
}