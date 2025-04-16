using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization.Metadata;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Serialization.Converter;

[RequiresUnreferencedCode(ProtoSerializer.SerializationUnreferencedCodeMessage)]
[RequiresDynamicCode(ProtoSerializer.SerializationRequiresDynamicCodeMessage)]
public class ProtoObjectConverter<T> : ProtoConverter<T>
{
    private readonly ProtoObjectInfo<T> _objectInfo = ProtoTypeResolver.CreateObjectInfo<T>();
    
    public override void Write(int field, WireType wireType, ProtoWriter writer, T value)
    {
        object? boxed = value; // avoid multiple times of boxing
        if (boxed is null) return;
        
        foreach (var (tag, info) in _objectInfo.Fields)
        {
            writer.EncodeVarInt(tag);
            info.Write(writer, boxed);
        }
    }

    public override int Measure(int field, WireType wireType, T value)
    {
        object? boxed = value; // avoid multiple times of boxing
        if (boxed is null) return 0;
        
        int result = 0;
        foreach (var (tag, info) in _objectInfo.Fields)
        {
            result += ProtoHelper.GetVarIntLength(tag);
            result += info.Measure(boxed);
        }
        
        return result;
    }

    public override T Read(int field, WireType wireType, ref ProtoReader reader)
    {
        Debug.Assert(_objectInfo.ObjectCreator != null);
        
        T target = _objectInfo.ObjectCreator();
        var boxed = (object?)target; // avoid multiple times of boxing
        if (boxed is null) ThrowHelper.ThrowInvalidOperationException_CanNotCreateObject(typeof(T));

        while (!reader.IsCompleted)
        {
            int tag = reader.DecodeVarIntUnsafe<int>();
            if (_objectInfo.Fields.TryGetValue(tag, out var fieldInfo))
            {
                fieldInfo.Read(fieldInfo.WireType, ref reader, boxed);
            }
            else
            {
                reader.SkipField((WireType)(tag & 0x07));
            }
        }
        
        return target;
    }
}