using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Service;

#pragma warning disable CS8618

[ProtoPackable]
internal partial class IncPullRequest
{
    [ProtoMember(2)] public uint ReqCount { get; set; } // req_count
    
    [ProtoMember(3)] public long Time { get; set; } // time
    
    [ProtoMember(4)] public uint LocalSeq { get; set; } // local_seq
    
    [ProtoMember(5)] public byte[]? Cookie { get; set; } // cookie
    
    [ProtoMember(6)] public int Flag { get; set; } // 1
    
    [ProtoMember(7)] public uint ProxySeq { get; set; } // proxy_seq
    
    [ProtoMember(10001)] public List<IncPullRequestBiz> RequestBiz { get; set; } // ext_busi
    
    [ProtoMember(10002)] public List<uint> ExtSnsFlagKey { get; set; } = []; // buddy_ext_sns_flag_codec.cc

    [ProtoMember(10003)] public List<uint> ExtPrivateIdListKey { get; set; } = []; // buddy_ext_pri_idlist_codec.cc
}

[ProtoPackable]
internal partial class IncPullRequestBiz
{
    [ProtoMember(1)] public int BizType { get; set; }
    
    [ProtoMember(2)] public IncPullRequestBizBusi BizData { get; set; } // known as key
}

[ProtoPackable]
internal partial class IncPullRequestBizBusi
{
    [ProtoMember(1)] public List<int> ExtBusi { get; set; } // known as key
}

[ProtoPackable]
internal partial class IncPullResponse
{
    [ProtoMember(1)] public uint Seq { get; set; } // BuddyListSeq
    
    [ProtoMember(2)] public byte[]? Cookie { get; set; } // BuddyListCookie
    
    [ProtoMember(3)] public bool IsEnd { get; set; } // BuddyListIsEnd
    
    [ProtoMember(6)] public long Time { get; set; } // BuddyListTime
    
    [ProtoMember(7)] public long SelfUin { get; set; }
    
    [ProtoMember(8)] public uint SmallSeq { get; set; } // BuddyListSmallSeq
    
    [ProtoMember(101)] public List<IncPullResponseFriend> FriendList { get; set; }
    
    [ProtoMember(102)] public List<IncPullResponseCategory> Category { get; set; }
}

[ProtoPackable]
internal partial class IncPullResponseFriend
{
    [ProtoMember(1)] public string Uid { get; set; }
    
    [ProtoMember(2)] public int CategoryId { get; set; } // from binary
    
    [ProtoMember(3)] public long Uin { get; set; }
    
    [ProtoMember(10001)] public Dictionary<int, IncPullResponseSubBiz> SubBiz { get; set; } // known as key
}

[ProtoPackable]
internal partial class IncPullResponseSubBiz
{
    [ProtoMember(1)] public Dictionary<int, int> NumData { get; set; }
    
    [ProtoMember(2)] public Dictionary<int, string> Data { get; set; }
}

[ProtoPackable]
internal partial class IncPullResponseCategory
{
    [ProtoMember(1)] public int CategoryId { get; set; } // from binary
    
    [ProtoMember(2)] public string CategoryName { get; set; } // from binary
    
    [ProtoMember(3)] public int CategoryMemberCount { get; set; } // from binary
    
    [ProtoMember(4)] public int CatogorySortId { get; set; } // from binary
}