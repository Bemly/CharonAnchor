using System.Collections.Concurrent;
using ProtoBuf.Meta;

namespace Lagrange.Core.Utility.Binary;

internal static class Protobuf
{
    private static readonly ConcurrentQueue<SegmentBufferWriter> BufferPool = new();
    
    private static readonly RuntimeTypeModel Serializer = RuntimeTypeModel.Create();

    static Protobuf()
    {
        Serializer.UseImplicitZeroDefaults = false; // don't use default values because QQ use proto2 that preserves default values
    }
    
    public static void Serialize<T>(ref BinaryPacket dest, T value)
    {
        if (!BufferPool.TryDequeue(out var writer))
        {
            writer = new SegmentBufferWriter();
        }
        
        Serializer.Serialize(writer, value);
        writer.WriteTo(ref dest);
        writer.Clear();
        
        BufferPool.Enqueue(writer);
    }
    
    public static ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        if (!BufferPool.TryDequeue(out var writer))
        {
            writer = new SegmentBufferWriter();
        }
        
        Serializer.Serialize(writer, value);
        var result = writer.CreateReadOnlyMemory();
        writer.Clear();
        BufferPool.Enqueue(writer);
        
        return result;
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> src) => Serializer.Deserialize<T>(src);
}