using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class SsoGetC2CMsgReq
{
    [ProtoMember(1)] public long? PeerUin { get; set; }
    
    [ProtoMember(2)] public string? PeerUid { get; set; }

    [ProtoMember(3)] public int StartSequence { get; set; }

    [ProtoMember(4)] public int EndSequence { get; set; }
}

[ProtoPackable]
internal partial class SsoGetC2CMsgRsp
{
    [ProtoMember(1)] public uint Retcode { get; set; }

    [ProtoMember(2)] public string Message { get; set; }
    
    [ProtoMember(3)] public uint StartSequence { get; set; }

    [ProtoMember(4)] public uint EndSequence { get; set; }
    
    [ProtoMember(7)] public List<CommonMessage> Messages { get; set; }
}