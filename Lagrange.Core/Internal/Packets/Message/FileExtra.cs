using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

[ProtoPackable]
internal partial class FileExtra
{
    [ProtoMember(1)] public NotOnlineFile? File { get; set; }
}