using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;

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
    
    internal static T DeserializeProtoPackableCore<T>(ref ProtoReader reader) where T : IProtoSerializable<T>
    {
        var objectInfo = T.TypeInfo;
        Debug.Assert(objectInfo.ObjectCreator != null);
        
        T target = objectInfo.ObjectCreator();

        while (!reader.IsCompleted)
        {
            int tag = reader.DecodeVarIntUnsafe<int>();
            var fieldInfo = objectInfo.Fields[tag];
            fieldInfo.Read(fieldInfo.WireType, ref reader, target);
        }
        
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
    public static T Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]  T>(ReadOnlySpan<byte> data)
    {
        throw new NotImplementedException();
    }
}