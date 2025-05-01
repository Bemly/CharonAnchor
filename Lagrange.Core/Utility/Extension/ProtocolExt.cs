using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;

namespace Lagrange.Core.Utility.Extension;

internal static class ProtocolExt
{
    public static bool IsAndroid(this Protocols protocol) => (protocol & ~Protocols.Android) == Protocols.None;
    
    public static bool IsPC(this Protocols protocol) => (protocol & ~Protocols.PC) == Protocols.None;

    public static bool IsGroup(this BotMessage message) => message.Contact is BotGroupMember;
}