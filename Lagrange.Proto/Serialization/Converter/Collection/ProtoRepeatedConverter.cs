using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Metadata;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Serialization.Converter.Collection;

public abstract class ProtoRepeatedConverter<TCollection, TElement> : ProtoConverter<TCollection> where TCollection : ICollection<TElement>
{
    private readonly ProtoConverter<TElement> _converter = ProtoTypeResolver.GetConverter<TElement>();
    
    public override void Write(int field, WireType wireType, ProtoWriter writer, TCollection value)
    {
        int tag = (field << 3) | (byte)wireType;
        
        foreach (var item in value)
        {
            writer.EncodeVarInt(tag);
            writer.EncodeVarInt(_converter.Measure(wireType, item));
            _converter.Write(field, wireType, writer, item);
        }
    }

    public override int Measure(WireType wireType, TCollection value)
    {
        throw new InvalidOperationException("Should not be called");
    }

    public override TCollection Read(int field, WireType wireType, ref ProtoReader reader)
    {
        var collection = Create();
        object? state = CreateState();
        
        int tag = reader.DecodeVarInt<int>();
        
        while (tag >> 3 == field)
        {
            var item = _converter.Read(field, wireType, ref reader);
            Add(item, collection, state);
            tag = reader.DecodeVarInt<int>();
        }

        reader.Rewind(ProtoHelper.GetVarIntLength(tag));

        return Finalize(collection, state);
    }

    private protected abstract TCollection Create();
    
    private protected virtual object? CreateState() => null;
    
    private protected abstract void Add(TElement item, TCollection collection, object? state);
    
    private protected virtual TCollection Finalize(TCollection collection, object? state) => collection;
}