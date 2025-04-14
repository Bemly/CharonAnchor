using Lagrange.Proto.Serialization.Converter;

namespace Lagrange.Proto.Serialization.Metadata;

public static partial class ProtoTypeResolver
{
    private static void RegisterWellKnownTypes()
    {
        Register(new ProtoVarIntConverter<SByte>());
        Register(new ProtoVarIntConverter<Byte>());
        Register(new ProtoVarIntConverter<Int16>());
        Register(new ProtoVarIntConverter<UInt16>());
        Register(new ProtoVarIntConverter<Int32>());
        Register(new ProtoVarIntConverter<UInt32>());
        Register(new ProtoVarIntConverter<Int64>());
        Register(new ProtoVarIntConverter<UInt64>());

        Register(new ProtoFixed32Converter<Single>());
        Register(new ProtoFixed32Converter<UInt32>());
        Register(new ProtoFixed32Converter<Int32>());

        Register(new ProtoFixed64Converter<Double>());
        Register(new ProtoFixed64Converter<UInt64>());
        Register(new ProtoFixed64Converter<Int64>());

        Register(new ProtoBooleanConverter());
        Register(new ProtoStringConverter());
        Register(new ProtoBytesConverter());
        Register(new ProtoReadOnlyMemoryByteConverter());
    }
}