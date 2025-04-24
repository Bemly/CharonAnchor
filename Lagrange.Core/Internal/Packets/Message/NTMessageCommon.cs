using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Message;

[ProtoPackable]
internal partial class CommonMessage
{
    [ProtoMember(1)] public RoutingHead RoutingHead { get; set; } = new();
    
    [ProtoMember(2)] public ContentHead ContentHead { get; set; } = new();
    
    [ProtoMember(3)] public MessageBody MessageBody { get; set; } = new();
}

[ProtoPackable]
internal partial class ContentHead
{
    [ProtoMember(1)] public long Type { get; set; }
    
    [ProtoMember(2)] public long SubType { get; set; }
    
    [ProtoMember(3)] public int C2CCommand { get; set; }
    
    [ProtoMember(4)] public long Random { get; set; }
    
    [ProtoMember(5)] public long MsgSeq { get; set; }
    
    [ProtoMember(6)] public long Time { get; set; }
    
    [ProtoMember(7)] public int PkgNum { get; set; }
    
    [ProtoMember(8)] public int PkgIndex { get; set; }
    
    [ProtoMember(9)] public int DivSeq { get; set; }
    
    [ProtoMember(10)] public int AutoReply { get; set; }
    
    [ProtoMember(11)] public long NtMsgSeq { get; set; }
    
    [ProtoMember(12)] public ulong MsgUid { get; set; }
}

[ProtoPackable]
internal partial class RoutingHead
{
    [ProtoMember(1)] public long FromUin { get; set; }

    [ProtoMember(2)] public string FromUid { get; set; } = string.Empty;

    [ProtoMember(3)] public int FromAppId { get; set; }

    [ProtoMember(4)] public int FromInstId { get; set; }

    [ProtoMember(5)] public long ToUin { get; set; }

    [ProtoMember(6)] public string ToUid { get; set; } = string.Empty;
    
    [ProtoMember(8)] public CommonGroup Group { get; set; } = new();
}

[ProtoPackable]
internal partial class CommonGroup
{
    [ProtoMember(1)] public long GroupCode { get; set; }

    [ProtoMember(2)] public int GroupType { get; set; }

    [ProtoMember(3)] public long GroupInfoSeq { get; set; }

    [ProtoMember(4)] public string GroupCard { get; set; } = string.Empty;

    [ProtoMember(5)] public int GroupCardType { get; set; }

    [ProtoMember(6)] public int GroupLevel { get; set; }

    [ProtoMember(7)] public string GroupName { get; set; } = string.Empty;

    [ProtoMember(8)] public string ExtGroupKeyInfo { get; set; } = string.Empty;

    [ProtoMember(9)] public int MsgFlag { get; set; }
}

[ProtoPackable]
internal partial class MessageBody
{
    [ProtoMember(1)] public RichText RichText { get; set; } = new();

    [ProtoMember(2)] public byte[] MsgContent { get; set; } = [];

    [ProtoMember(3)] public byte[] MsgEncryptContent { get; set; } = [];
}

[ProtoPackable]
internal partial class RichText
{
    [ProtoMember(1)] public Attr Attr { get; set; } = new();
}

[ProtoPackable]
internal partial class Attr
{
    [ProtoMember(1)] public int CodePage { get; set; }

    [ProtoMember(2)] public int Time { get; set; }

    [ProtoMember(3)] public int PlayModeRandom { get; set; }

    [ProtoMember(4)] public int Color { get; set; }

    [ProtoMember(5)] public int Size { get; set; }

    [ProtoMember(6)] public int TabEffect { get; set; }

    [ProtoMember(7)] public int CharSet { get; set; }

    [ProtoMember(8)] public int PitchAndFamily { get; set; }

    [ProtoMember(9)] public string FontName { get; set; } = "Times New Roman";

    [ProtoMember(10)] public byte[] ReserveData { get; set; } = [];
}