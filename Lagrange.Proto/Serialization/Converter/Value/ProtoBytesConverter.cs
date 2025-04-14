using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

public class ProtoBytesConverter : ProtoConverter<byte[]>
{
    public override WireType WireType => WireType.LengthDelimited;
    
    public override void Write(int field, ProtoWriter writer, byte[] value)
    {
        writer.EncodeVarInt(value.Length);
        writer.WriteRawBytes(value);
    }

    public override byte[] Read(int field, ref ProtoReader reader)
    {
        int length = reader.DecodeVarInt<int>();
        if (length == 0) return [];

        var buffer = GC.AllocateUninitializedArray<byte>(length);
        var span = reader.CreateSpan(length);
        span.CopyTo(buffer);
        
        return buffer;
    }
}