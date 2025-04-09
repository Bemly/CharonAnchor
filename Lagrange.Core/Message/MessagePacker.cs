using System.Diagnostics.CodeAnalysis;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Message;

internal class MessagePacker
{
    private readonly BotContext _context;
    
    private readonly List<IMessageEntity> _factory;
    
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public MessagePacker(BotContext context)
    {
        _context = context;
        _factory = [];

        foreach (var type in typeof(MessagePacker).Assembly.GetTypes())
        {
            if (type.HasImplemented<IMessageEntity>())
            {
                _factory.Add((IMessageEntity?)Activator.CreateInstance(type) ?? throw new InvalidOperationException());
            }
        }
    }

    public BotMessage Parse(ReadOnlySpan<byte> src)
    {
        throw new NotImplementedException();
    }

    public Task<ReadOnlyMemory<byte>> Build(BotMessage message)
    {
        throw new NotImplementedException();
    }
}