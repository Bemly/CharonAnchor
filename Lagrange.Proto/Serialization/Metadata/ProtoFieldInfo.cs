namespace Lagrange.Proto.Serialization.Metadata;

public class ProtoFieldInfo
{
    public int Field { get; }
    
    public WireType WireType { get; }
    
    public Func<object> ObjectCreationDelegate { get; }
    
    public ProtoConverter Converter { get; }
}

public class ProtoFieldInfo<T> : ProtoFieldInfo
{
    public Func<T> TypedObjectCreationDelegate { get; }
    
    public ProtoConverter<T> TypedConverter { get; }
}