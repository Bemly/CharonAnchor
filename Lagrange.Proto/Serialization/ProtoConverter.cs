namespace Lagrange.Proto.Serialization;

public abstract class ProtoConverter
{
    
}

public abstract class ProtoConverter<T> : ProtoConverter
{
    public abstract void Write(int field, T value);
    
    public abstract T Read(int field);
}