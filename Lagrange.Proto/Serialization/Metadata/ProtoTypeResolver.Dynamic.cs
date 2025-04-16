using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Lagrange.Proto.Serialization.Metadata;

public static partial class ProtoTypeResolver
{
    private static readonly MethodInfo PopulateFieldInfoMethod = typeof(ProtoTypeResolver).GetMethod(nameof(PopulateFieldInfo), BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException($"Unable to find method {nameof(PopulateFieldInfo)} in {nameof(ProtoTypeResolver)}");

    private static MemberAccessor MemberAccessor
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
    internal static ProtoObjectInfo<T> CreateObjectInfo<T>()
    {
        var ctor = typeof(T).IsValueType ? null : typeof(T).GetConstructor(Type.EmptyTypes);
        bool ignoreDefaultFields = typeof(T).GetCustomAttribute<ProtoPackableAttribute>()?.IgnoreDefaultFields == true;
        var fields = new Dictionary<int, ProtoFieldInfo>();
        
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.IsStatic) continue;
            var fieldInfo = CreateFieldInfo(typeof(T), field);
            if (fieldInfo == null) continue;
            
            if (fields.ContainsKey(fieldInfo.Field)) ThrowHelper.ThrowInvalidOperationException_DuplicateField(typeof(T), fieldInfo.Field);
            fields[(fieldInfo.Field << 3) | (byte)fieldInfo.WireType] = fieldInfo;
        }
        
        foreach (var field in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var fieldInfo = CreateFieldInfo(typeof(T), field);
            if (fieldInfo == null) continue;
            
            if (fields.ContainsKey(fieldInfo.Field)) ThrowHelper.ThrowInvalidOperationException_DuplicateField(typeof(T), fieldInfo.Field);
            fields[(fieldInfo.Field << 3) | (byte)fieldInfo.WireType] = fieldInfo;
        }
        
        return new ProtoObjectInfo<T>
        {
            ObjectCreator = MemberAccessor.CreateParameterlessConstructor<T>(ctor),
            IgnoreDefaultFields = ignoreDefaultFields,
            Fields = fields
        };
    }


    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    internal static ProtoFieldInfo? CreateFieldInfo(Type declared, MemberInfo member)
    {
        Debug.Assert(member.DeclaringType != null);
        var targetType = member switch
        {
            PropertyInfo p => p.PropertyType,
            FieldInfo f => f.FieldType,
            _ => throw new NotSupportedException($"Unsupported member type: {member.MemberType}")
        };
        
        var method = PopulateFieldInfoMethod.MakeGenericMethod(targetType);
        return (ProtoFieldInfo?)method.Invoke(null, [declared, member]);
    }


    [RequiresUnreferencedCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    internal static ProtoFieldInfo<TField>? PopulateFieldInfo<TField>(Type declared, MemberInfo member)
    {
        var type = typeof(TField);
        var attribute = member.GetCustomAttribute<ProtoMemberAttribute>();
        if (attribute == null) return null;
        
        var wireType = DetermineWireType(type, attribute.NumberHandling);
        var fieldInfo = new ProtoFieldInfo<TField>(attribute.Field, wireType, declared);
        
        DetermineAccessors(fieldInfo, member, false);

        return fieldInfo;
    }
    
    [RequiresUnreferencedCode(ProtoSerializer.SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
    private static void DetermineAccessors<T>(ProtoFieldInfo<T> jsonPropertyInfo, MemberInfo memberInfo, bool useNonPublicAccessors)
    {
        Debug.Assert(memberInfo is FieldInfo or PropertyInfo);

        switch (memberInfo)
        {
            case PropertyInfo propertyInfo:
            {
                var getMethod = propertyInfo.GetMethod;
                if (getMethod != null && (getMethod.IsPublic || useNonPublicAccessors)) jsonPropertyInfo.Get = MemberAccessor.CreatePropertyGetter<T>(propertyInfo);
                
                var setMethod = propertyInfo.SetMethod;
                if (setMethod != null && (setMethod.IsPublic || useNonPublicAccessors)) jsonPropertyInfo.Set = MemberAccessor.CreatePropertySetter<T>(propertyInfo);
                break;
            }
            case FieldInfo fieldInfo:
            {
                Debug.Assert(fieldInfo.IsPublic || useNonPublicAccessors);

                jsonPropertyInfo.Get = MemberAccessor.CreateFieldGetter<T>(fieldInfo);
                if (!fieldInfo.IsInitOnly) jsonPropertyInfo.Set = MemberAccessor.CreateFieldSetter<T>(fieldInfo);
                break;
            }
            default:
            {
                Debug.Fail($"Invalid MemberInfo type: {memberInfo.MemberType}");
                break;
            }
        }
    }

    private static WireType DetermineWireType(Type fieldType, ProtoNumberHandling numberHandling)
    {
        if (fieldType.IsEnum) fieldType = Enum.GetUnderlyingType(fieldType);

        if (fieldType is { IsValueType: true, IsGenericType: true })
        {
            var genericType = fieldType.GetGenericTypeDefinition();
            if (genericType == typeof(Nullable<>))
            {
                fieldType = Nullable.GetUnderlyingType(fieldType)!;
            }
        }

        var result = fieldType switch
        {
            { IsEnum: true } => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(bool) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(byte) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(sbyte) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(short) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(ushort) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(int) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(uint) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(long) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(ulong) => WireType.VarInt,
            { IsValueType: true } when fieldType == typeof(float) => WireType.Fixed32,
            { IsValueType: true } when fieldType == typeof(double) => WireType.Fixed64,
            _ => WireType.LengthDelimited
        };

        if (numberHandling != default)
        {
            if (result != WireType.VarInt) ThrowHelper.ThrowInvalidOperationException_InvalidNumberHandling(fieldType);

            if ((numberHandling & ProtoNumberHandling.Fixed32) != 0) result = WireType.Fixed32;
            else if ((numberHandling & ProtoNumberHandling.Fixed64) != 0) result = WireType.Fixed64;
        }

        return result;
    }
}