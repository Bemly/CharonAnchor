using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Converter;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Serialization;

public static partial class ProtoSerializer
{
    /// <summary>
    /// Deserialize the ProtoPackable Object from the source buffer, AOT Friendly
    /// </summary>
    /// <param name="data">The source buffer to read from</param>
    /// <typeparam name="T">The type of the object to deserialize</typeparam>
    /// <returns>The deserialized object</returns>
    public static T DeserializeProtoPackable<T>(ReadOnlySpan<byte> data) where T : IProtoSerializable<T>
    {
        var reader = new ProtoReader(data);
        return DeserializeProtoPackableCore<T>(ref reader);
    }
    
    private static T DeserializeProtoPackableCore<T>(ref ProtoReader reader) where T : IProtoSerializable<T>
    {
        var objectInfo = T.TypeInfo;
        Debug.Assert(objectInfo.ObjectCreator != null);
        
        T target = objectInfo.ObjectCreator();

        while (!reader.IsCompleted)
        {
            int tag = reader.DecodeVarIntUnsafe<int>();
            var fieldInfo = objectInfo.Fields[tag];
            fieldInfo.Read(fieldInfo.WireType, ref reader, target);
        } // TODO: Handle unknown fields
        
        return target;
    }
    
    /// <summary>
    /// Deserialize the ProtoPackable Object from the source buffer, based on reflection
    /// </summary>
    /// <param name="data">The source buffer to read from</param>
    /// <typeparam name="T">The type of the object to deserialize</typeparam>
    /// <returns>The deserialized object</returns>
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ReadOnlySpan<byte> data)
    {
        var reader = new ProtoReader(data);
        return DeserializeCore<T>(ref reader);
    }
    
    [RequiresUnreferencedCode(SerializationUnreferencedCodeMessage)]
    [RequiresDynamicCode(SerializationRequiresDynamicCodeMessage)]
    private static T DeserializeCore<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(ref ProtoReader reader)
    {
        ProtoConverter<T> converter;
        if (ProtoTypeResolver.IsRegistered<T>())
        {
            converter = ProtoTypeResolver.GetConverter<T>();
        }
        else
        {
            converter = new ProtoObjectConverter<T>();
            ProtoTypeResolver.Register(converter);
        }
        
        return converter.Read(0, WireType.VarInt, ref reader);
    }
}