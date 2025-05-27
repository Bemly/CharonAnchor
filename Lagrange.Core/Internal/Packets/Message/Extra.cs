using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class FileExtra
{
    [ProtoMember(1)] public NotOnlineFile? File { get; set; }
}

[ProtoPackable]
internal partial class TextResvAttr
{
    [ProtoMember(1)] public string? Wording { get; set; }
    
    [ProtoMember(2)] public uint TextAnalysisResult { get; set; }
    
    [ProtoMember(3)] public uint AtType { get; set; }
    
    [ProtoMember(4)] public ulong AtMemberUin { get; set; }
    
    [ProtoMember(5)] public ulong AtMemberTinyid { get; set; }
    
    [ProtoMember(6)] public RoleInfo? AtMemberRoleInfo { get; set; }
    
    [ProtoMember(7)] public RoleInfo? AtRoleInfo { get; set; }
    
    [ProtoMember(8)] public ChannelInfo? AtChannelInfo { get; set; }
    
    [ProtoMember(9)] public string? AtMemberUid { get; set; }
}

[ProtoPackable]
internal partial class ChannelInfo
{
    [ProtoMember(1)] public ulong GuildId { get; set; }
    
    [ProtoMember(2)] public ulong ChannelId { get; set; }
}

[ProtoPackable]
internal partial class RoleInfo;

[ProtoPackable]
internal partial class SourceMsgResvAttr
{
    [ProtoMember(3)] public ulong SourceMsgId { get; set; }
    
    [ProtoMember(6)] public string SenderUid { get; set; }
    
    [ProtoMember(7)] public string ReceiverUid { get; set; }
}