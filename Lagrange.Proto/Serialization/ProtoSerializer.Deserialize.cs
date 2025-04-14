using System.Buffers;
using System.Diagnostics.CodeAnalysis;

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
        throw new NotImplementedException();
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