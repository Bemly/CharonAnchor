using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization;

public abstract class ProtoConverter
{
    public abstract WireType WireType { get; }
}

public abstract class ProtoConverter<T> : ProtoConverter
{
    public abstract void Write(int field, ProtoWriter writer, T value);
    
    public abstract T Read(int field, ref ProtoReader reader);
}