using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Services;

internal abstract class BaseService<TReq, TResp> : IService where TReq : ProtocolEvent where TResp : ProtocolEvent
{
    protected virtual ValueTask<TResp?> Parse(ReadOnlyMemory<byte> input, BotContext context) => ValueTask.FromResult<TResp?>(null);
    
    protected virtual ValueTask<ReadOnlyMemory<byte>> Build(TReq input, BotContext context) => ValueTask.FromResult(ReadOnlyMemory<byte>.Empty);
    
    async ValueTask<ProtocolEvent?> IService.Parse(ReadOnlyMemory<byte> input, BotContext context) => await Parse(input, context);
    
    ValueTask<ReadOnlyMemory<byte>> IService.Build(ProtocolEvent input, BotContext context) => Build((TReq)input, context);
}