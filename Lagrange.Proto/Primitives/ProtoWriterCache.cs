using System.Buffers;
using System.Diagnostics;

namespace Lagrange.Proto.Primitives;

/// <summary>
/// Defines a thread-local cache for ProtoSerializer to store reusable ProtoWriterCache instances.
/// </summary>
internal static class ProtoWriterCache
{
    [ThreadStatic]
    private static ThreadLocalState? _threadLocalState;
    
    public static ProtoWriter RentWriter(IBufferWriter<byte> bufferWriter)
    {
        var state = _threadLocalState ??= new ThreadLocalState();
        ProtoWriter writer;

        if (state.RentedWriters++ == 0)
        {
            writer = state.Writer;
            writer.Reset(bufferWriter);
        }
        else
        {
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