using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;
using Lagrange.Proto.Serialization.Metadata;

namespace Lagrange.Proto.Nodes;

public abstract partial class ProtoValue(WireType wireType) : ProtoNode(wireType)
{
    public abstract bool TryGetValue<T>([NotNullWhen(true)] out T? value);
}

public sealed class ProtoValue<TValue> : ProtoValue
{
    internal readonly TValue Value; // keep as a field for direct access to avoid copies

    private readonly ProtoConverter<TValue> _converter;
    
    public ProtoValue(TValue value, WireType wireType) : base(wireType)
    {
        Debug.Assert(value != null);
        Debug.Assert(value is not ProtoNode);
        
        Value = value;
        _converter = ProtoTypeResolver.GetConverter<TValue>();
    }

    public override bool TryGetValue<T>([NotNullWhen(true)] out T? value) where T : default
    {
        throw new NotImplementedException();
    }

    public override void WriteTo(int field, ProtoWriter writer)
    {
        _converter.Write(field, WireType, writer, Value);
    }
}