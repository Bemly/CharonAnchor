using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoErrorConverter<T> : ProtoConverter<T>
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, T value)
    {
        throw new InvalidOperationException($"Unable to determine the type of the object to serialize for {typeof(T).Name}");
    }

    public override int Measure(WireType wireType, T value)
    {
        throw new InvalidOperationException($"Unable to determine the type of the object to serialize for {typeof(T).Name}");
    }

    public override T Read(int field, WireType wireType, ref ProtoReader reader)
    {
        throw new InvalidOperationException($"Unable to determine the type of the object to serialize for {typeof(T).Name}");
    }
}