using System.Diagnostics.CodeAnalysis;

namespace Lagrange.Proto.Serialization.Metadata;

public static partial class ProtoTypeResolver
{
    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    private static ProtoObjectInfo<T> CreateObjectInfo<T>()
    {
        throw new NotImplementedException();
    }
    
    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    private static ProtoFieldInfo<TField> CreateFieldInfo<TObject, TField>()
    {
        throw new NotImplementedException();
    }
}