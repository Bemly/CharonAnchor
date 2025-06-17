using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

/// <summary>
/// trpc.msg.register_proxy.RegisterProxy.SsoGetRoamMsg
/// </summary>
[ProtoPackable]
internal partial class SsoGetRoamMsgReq
{
    [ProtoMember(1)] public string? PeerUid { get; set; }
    
    [ProtoMember(2)] public uint Time { get; set; }
    
    [ProtoMember(3)] public uint Random { get; set; }  // 0
    
    [ProtoMember(4)] public uint Count { get; set; } // max 30
    
    [ProtoMember(5)] public uint Direction { get; set; } // 1 for upwards, (default) 2 for downwards
}

[ProtoPackable]
internal partial class SsoGetRoamMsgRsp
{
    [ProtoMember(3)] public string? PeerUid { get; set; }
    
    [ProtoMember(4)] public bool IsComplete { get; set; }
    
    [ProtoMember(5)] public uint Timestamp { get; set; }

    [ProtoMember(6)] public uint Random { get; set; }

    [ProtoMember(7)] public List<CommonMessage> Messages { get; set; } = [];
}