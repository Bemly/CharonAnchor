using Lagrange.Proto.Primitives;

namespace Lagrange.Proto;

public interface IProtoSerializer<in T>
{
    public static abstract void SerializeHandler(T obj, ProtoWriter writer);
    
    public static abstract int MeasureHandler(T obj);
}