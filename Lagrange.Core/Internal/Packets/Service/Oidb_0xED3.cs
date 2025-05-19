using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class DED3ReqBody
{
    [ProtoMember(1)] public long ToUin { get; set; } // uint64_to_uin
    
    [ProtoMember(2)] public long GroupCode { get; set; } // uint64_group_code
    
    [ProtoMember(3)] public uint MsgSeq { get; set; } // uint32_msg_seq
    
    [ProtoMember(4)] public uint MsgRand { get; set; } // uint32_msg_rand
    
    [ProtoMember(5)] public long AioUin { get; set; } // uint64_aio_uin
    
    [ProtoMember(6)] public uint NudgeType { get; set; } // uint32_nudge_type
}

[ProtoPackable]
internal partial class DED3RspBody;