using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Services;

/// <summary>
/// Derive from this class to create a service that handles Oidb packets, enable automatic marshalling and unmarshalling of packets and error handling.
/// </summary>
internal abstract class OidbService<TEventReq, TEventResp, TRequest, TResponse> : BaseService<TEventReq, TEventResp> 
    where TEventReq : ProtocolEvent 
    where TEventResp : ProtocolEvent
{
    protected abstract Task<ProtocolEvent?> ProcessRequest(TRequest request, BotContext context);
    
    protected abstract Task<TResponse> ProcessResponse(TResponse response, BotContext context);
    
    protected override ValueTask<TEventResp?> Parse(ReadOnlyMemory<byte> input, BotContext context)
    {
        throw new NotImplementedException();
    }

    protected override ValueTask<ReadOnlyMemory<byte>> Build(TEventReq input, BotContext context)
    {
        throw new NotImplementedException();
    }
}