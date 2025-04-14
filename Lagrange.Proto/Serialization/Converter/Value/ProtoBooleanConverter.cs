using System.Runtime.CompilerServices;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Converter;

internal class ProtoBooleanConverter : ProtoConverter<bool>
{
    public override WireType WireType => WireType.VarInt;
    
    public override void Write(int field, ProtoWriter writer, bool value)
    {
        writer.WriteRawByte(Unsafe.As<bool, byte>(ref value));
    }

    public override bool Read(int field, ref ProtoReader reader)
    {
        byte b = reader.DecodeVarInt<byte>();
        return Unsafe.As<byte, bool>(ref b);
    }
}