using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization;

public abstract class ProtoConverter
{
}

public abstract class ProtoConverter<T> : ProtoConverter
{
    public abstract void Write(int field, WireType wireType, ProtoWriter writer, T value);
    
    public abstract T Read(int field, WireType wireType, ref ProtoReader reader);
}