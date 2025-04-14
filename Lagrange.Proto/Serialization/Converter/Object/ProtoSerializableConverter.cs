using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter.Object;

internal class ProtoSerializableConverter<T> : ProtoConverter<T> where T : IProtoSerializable<T>
{
    public override void Write(int field, WireType wireType, ProtoWriter writer, T value)
    {
        int length = T.MeasureHandler(value);
        writer.EncodeVarInt(length);
        if (length > 0) T.SerializeHandler(value, writer);
    }

    public override T Read(int field, WireType wireType, ref ProtoReader reader)
    {
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        return ProtoSerializer.DeserializeProtoPackable<T>(span);
    }
}