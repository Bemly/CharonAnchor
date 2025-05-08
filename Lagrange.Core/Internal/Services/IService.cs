using Lagrange.Core.Internal.Events;

namespace Lagrange.Core.Internal.Services;

internal interface IService
{
    public ValueTask<ProtocolEvent> Parse(ReadOnlyMemory<byte> input, BotContext context);
    
    public ValueTask<ReadOnlyMemory<byte>> Build(ProtocolEvent input, BotContext context);
}