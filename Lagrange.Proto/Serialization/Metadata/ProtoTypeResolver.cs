using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Lagrange.Proto.Serialization.Converter.Generic;

namespace Lagrange.Proto.Serialization.Metadata;

[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static partial class ProtoTypeResolver
{
    private static readonly ConcurrentDictionary<Type, ProtoConverter> Converters = new(Environment.ProcessorCount, 150);

    static ProtoTypeResolver()
    {
        RegisterWellKnownTypes();
    }

    private static readonly Dictionary<Type, Type> KnownGenericTypeConverters = new(3)
    {
        { typeof(Nullable<>), typeof(ProtoNullableConverter<>) },
    };
    
    public static void Register<T>(ProtoConverter<T> converter)
    {
        Check<T>.Registered = true; // avoid to call Cache() constructor called.
        Converters[typeof(T)] = converter;
        Cache<T>.Converter = converter;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRegistered<T>() => Check<T>.Registered;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ProtoConverter<T> GetConverter<T>()
    {
        return Cache<T>.Converter;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ProtoConverter GetConverter(Type type)
    {
        if (Converters.TryGetValue(type, out var converter))
        {
            return converter;
        }

        throw new NotImplementedException();
    }

    private static class Cache<T>
    {
        public static ProtoConverter<T> Converter;
        
        static Cache()
        {
            if (Check<T>.Registered) return;

            var type = typeof(T);
            if ((type.IsGenericType) && ResolveGenericConverter<T>(type) is { } converter)
            {
                Converters[type] = converter;
                Converter = converter;
            }

            Check<T>.Registered = true;
        }
    }

    [UnconditionalSuppressMessage("Trimmer", "IL3050", Justification = "The generic type definition would always appear in metadata as it is a member in class serialized.")]
    private static ProtoConverter<T>? ResolveGenericConverter<T>(Type type)
    {
        if (KnownGenericTypeConverters.TryGetValue(type.GetGenericTypeDefinition(), out var converterType))
        {
            var args = type.GenericTypeArguments;
            var converter = converterType.MakeGenericType(args);
            return (ProtoConverter<T>)Activator.CreateInstance(converter)!;
        }

        return null;
    }
    
    private static class Check<T>
    {
        public static bool Registered { get; set; }
    }
}