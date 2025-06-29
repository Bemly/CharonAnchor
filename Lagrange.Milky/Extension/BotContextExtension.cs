using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Lagrange.Core;

namespace Lagrange.Milky.Extension;

public static class BotContextExtension
{
    private static Func<BotContext, string, ReadOnlyMemory<byte>, int, ValueTask<(ReadOnlyMemory<byte> Data, string Command, int Sequence, int RetCode, string Extra)>> _callSendPacket;

    static BotContextExtension()
    {
        // TODO: LWX Look me!
        _callSendPacket = null!;
    }

    public static ValueTask<(ReadOnlyMemory<byte> Data, string Command, int Sequence, int RetCode, string Extra)> SendPacket(this BotContext bot, string cmd, ReadOnlyMemory<byte> payload, int seq)
    {
        return _callSendPacket(bot, cmd, payload, seq);
    }
}