using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

#pragma warning disable CS8618

/// <summary>
/// trpc.msg.register_proxy.RegisterProxy.SsoGetGroupMsg
/// </summary>
[ProtoPackable]
internal partial class SsoGetGroupMsg
{
    [ProtoMember(1)] public SsoGetGroupMsgInfo Info { get; set; }
    
    [ProtoMember(2)] public bool Direction { get; set; }
}

[ProtoPackable]
internal partial class SsoGetGroupMsgInfo
{
    [ProtoMember(1)] public long GroupUin { get; set; }
    
    [ProtoMember(2)] public int StartSequence { get; set; }
    
    [ProtoMember(3)] public int EndSequence { get; set; }
}

[ProtoPackable]
internal partial class SsoGetGroupMsgRsp
{
    [ProtoMember(1)] public uint RetCode { get; set; }
    
    [ProtoMember(2)] public string ErrorMsg { get; set; }
    
    [ProtoMember(3)] public SsoGetGroupMsgRspBody Body { get; set; }
}

[ProtoPackable]
internal partial class SsoGetGroupMsgRspBody
{
    [ProtoMember(1)] public uint Retcode { get; set; }

    [ProtoMember(2)] public string Message { get; set; }

    [ProtoMember(3)] public uint GroupUin { get; set; }

    [ProtoMember(4)] public uint StartSequence { get; set; }

    [ProtoMember(5)] public uint EndSequence { get; set; }

    [ProtoMember(6)] public List<CommonMessage>? Messages { get; set; }
}