using System.Text;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

internal class ProtoStringConverter : ProtoConverter<string>
{
    public override WireType WireType => WireType.LengthDelimited;
    
    public override void Write(int field, ProtoWriter writer, string value)
    { 
        writer.EncodeString(value);
    }

    public override string Read(int field, ref ProtoReader reader)
    {
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        if (span.IsEmpty) return string.Empty;
        
        return Encoding.UTF8.GetString(span);
    }
}