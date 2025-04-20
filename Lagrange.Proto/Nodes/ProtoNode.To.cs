using System.Buffers;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Nodes;

public partial class ProtoNode
{
    public byte[] Serialize()
    {
        if (this is ProtoArray) ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(ProtoObject), nameof(ProtoValue));
        
        var writer = ProtoWriterCache.RentWriterAndBuffer(512, out var buffer);
        try
        {
            WriteTo(0, writer);
            writer.Flush();

            return buffer.ToArray();
        }
        finally
        {
            ProtoWriterCache.ReturnWriterAndBuffer(writer, buffer);
        }
    }

    public void Serialize(IBufferWriter<byte> buffer)
    {
        if (this is ProtoArray) ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(ProtoObject), nameof(ProtoValue));
        
        var writer = ProtoWriterCache.RentWriter(buffer);
        try
        {
            WriteTo(0, writer);
            writer.Flush();
        }
        finally
        {
            ProtoWriterCache.ReturnWriter(writer);
        }
    }
    
    public abstract void WriteTo(int field, ProtoWriter writer);
}