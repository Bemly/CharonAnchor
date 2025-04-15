using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Lagrange.Proto.Serialization.Metadata;

[RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
internal sealed class ReflectionMemberAccessor : MemberAccessor
{
    public override Func<object>? CreateParameterlessConstructor(Type type, ConstructorInfo? ctorInfo)
    {
        Debug.Assert(type != null);
        Debug.Assert(ctorInfo is null || ctorInfo.GetParameters().Length == 0);

        if (type.IsAbstract) return null;

        return ctorInfo is null
            ? type.IsValueType ? () => Activator.CreateInstance(type, nonPublic: false)! : null
            : () => ctorInfo.Invoke(null);
    }
    
    public override Func<object, TProperty> CreatePropertyGetter<TProperty>(PropertyInfo propertyInfo)
    {
        var getMethodInfo = propertyInfo.GetMethod!;

        return obj => (TProperty)getMethodInfo.Invoke(obj, null)!;
    }

    public override Action<object, TProperty> CreatePropertySetter<TProperty>(PropertyInfo propertyInfo)
    {
        var setMethodInfo = propertyInfo.SetMethod!;

        return delegate (object obj, TProperty value) { setMethodInfo.Invoke(obj, [value!]); };
    }

    public override Func<object, TProperty> CreateFieldGetter<TProperty>(FieldInfo fieldInfo) =>
        obj => (TProperty)fieldInfo.GetValue(obj)!;

    public override Action<object, TProperty> CreateFieldSetter<TProperty>(FieldInfo fieldInfo) =>
        delegate (object obj, TProperty value) { fieldInfo.SetValue(obj, value); };
}