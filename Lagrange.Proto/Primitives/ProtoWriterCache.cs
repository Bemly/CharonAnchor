using System.Buffers;
using System.Diagnostics;

namespace Lagrange.Proto.Primitives;

/// <summary>
/// Defines a thread-local cache for JsonSerializer to store reusable Utf8JsonWriter/IBufferWriter instances.
/// </summary>
internal static class Utf8JsonWriterCache
{
    [ThreadStatic]
    private static ThreadLocalState? _threadLocalState;
    
    public static ProtoWriter RentWriter(IBufferWriter<byte> bufferWriter)
    {
        var state = _threadLocalState ??= new ThreadLocalState();
        ProtoWriter writer;

        if (state.RentedWriters++ == 0)
        {
            // First JsonSerializer call in the stack -- initialize & return the cached instance.
            writer = state.Writer;
            writer.Reset(bufferWriter);
        }
        else
        {
            // We're in a recursive JsonSerializer call -- return a fresh instance.
            writer = new ProtoWriter(bufferWriter);
        }

        return writer;
    }
        
    public static void ReturnWriter(ProtoWriter writer)
    {
        Debug.Assert(_threadLocalState != null);
            
        writer.ResetAllStateForCacheReuse();

        int rentedWriters = --_threadLocalState.RentedWriters;
        Debug.Assert((rentedWriters == 0) == ReferenceEquals(_threadLocalState.Writer, writer));
    }

    private sealed class ThreadLocalState
    {
        public readonly ProtoWriter Writer = new();
        public int RentedWriters;
    }
}