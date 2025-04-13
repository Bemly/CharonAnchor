using System.Diagnostics;

namespace Lagrange.Proto.Serialization.Metadata;

[DebuggerDisplay("Fields = {Fields.Count}")]
public class ProtoObjectInfo<T> : ProtoTypeInfo<T>
{
    public Dictionary<int, ProtoFieldInfo> Fields { get; init; } = new();
}