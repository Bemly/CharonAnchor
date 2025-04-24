using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

[ProtoPackable]
internal partial class MsgPush
{
    [ProtoMember(1)] public Message MessageBody { get; set; } = new();
    
    [ProtoMember(5)] public bool PushNotifyFlag { get; set; }
}