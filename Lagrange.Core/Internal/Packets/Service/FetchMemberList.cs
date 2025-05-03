using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

[ProtoPackable]
internal partial class FetchMemberList
{
    [ProtoMember(15)] public string? Token { get; set; }
}

[ProtoPackable]
internal partial class FetchMemberListResponse
{
    [ProtoMember(1)] public long GroupUin { get; set; } // group_code

    [ProtoMember(3)] public uint MemberCount { get; set; } // member_count

    [ProtoMember(5)] public uint MemberListChangeSeq { get; set; } // member_list_change_seq

    [ProtoMember(6)] public uint MemberCardSeq { get; set; } // member_card_seq
    
    [ProtoMember(15)] public string? Token { get; set; }
}