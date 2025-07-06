using System.Text;
using Lagrange.Core.Message;

namespace Lagrange.Milky.Extension;

public static class MessageChainExtension
{
    public static string ToDebugString(this MessageChain messages)
    {
        return new StringBuilder().AppendJoin(' ', messages.Select(MessageEntityExtension.ToDebugString)).ToString();
    }
}