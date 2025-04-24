using Lagrange.Core.Common;

namespace Lagrange.Core.Utility.Extension;

internal static class ProtocolExt
{
    public static bool IsAndroid(this Protocols protocol) => (protocol & ~Protocols.Android) == Protocols.None;
    
    public static bool IsPC(this Protocols protocol) => (protocol & ~Protocols.PC) == Protocols.None;
}