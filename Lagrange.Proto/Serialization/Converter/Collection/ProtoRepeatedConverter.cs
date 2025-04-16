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
            writer.EncodeVarInt(_converter.Measure(field, wireType, item));
            _converter.Write(field, wireType, writer, item);
        }
    }

    public override void WriteWithNumberHandling(int field, WireType wireType, ProtoWriter writer, TCollection value, ProtoNumberHandling numberHandling)
    {
        int tag = (field << 3) | (byte)wireType;
        
        foreach (var item in value)
        {
            writer.EncodeVarInt(tag);
            writer.EncodeVarInt(_converter.Measure(field, wireType, item));
            _converter.WriteWithNumberHandling(field, wireType, writer, item, numberHandling);
        }
    }

    public override int Measure(int field, WireType wireType, TCollection value)
    {
        int tag = (field << 3) | (byte)wireType;
        int size = ProtoHelper.GetVarIntLength(tag) * (value.Count - 1); // the length of the first item is not counted as it would be added by the caller
        
        foreach (var item in value)
        {
            size += ProtoHelper.GetVarIntLength(_converter.Measure(field, wireType, item));
            size += _converter.Measure(field, wireType, item);
        }

        return size;
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
    
    public override TCollection ReadWithNumberHandling(int field, WireType wireType, ref ProtoReader reader, ProtoNumberHandling numberHandling)
    {
        var collection = Create();
        object? state = CreateState();
        
        int tag = reader.DecodeVarInt<int>();
        
        while (tag >> 3 == field)
        {
            var item = _converter.ReadWithNumberHandling(field, wireType, ref reader, numberHandling);
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