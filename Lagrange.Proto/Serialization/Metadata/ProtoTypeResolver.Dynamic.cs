using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Lagrange.Proto.Serialization.Metadata;

public static partial class ProtoTypeResolver
{
    internal static MemberAccessor MemberAccessor
    {
        [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
        [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
        get
        {
            return _memberAccessor ?? Initialize();
            static MemberAccessor Initialize()
            {
                MemberAccessor value = RuntimeFeature.IsDynamicCodeSupported ?
                    new ReflectionEmitCachingMemberAccessor() : 
                    new ReflectionMemberAccessor();

                return Interlocked.CompareExchange(ref _memberAccessor, value, null) ?? value;
            }
        }
    }
    
    private static MemberAccessor? _memberAccessor;
    
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