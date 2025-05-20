using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class KickNTReq
{
    [ProtoMember(1)] public long Uin { get; set; }
    
    [ProtoMember(2)] public bool IsSameDevice { get; set; }
    
    [ProtoMember(3)] public string TipsInfo { get; set; }
    
    [ProtoMember(4)] public string TipsTitle { get; set; }
}