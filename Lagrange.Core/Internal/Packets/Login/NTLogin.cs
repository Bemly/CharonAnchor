using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Login;

[ProtoPackable]
internal partial class NTLoginAccount
{
    [ProtoMember(1)] public string Account { get; set; } = string.Empty;
    
    // [ProtoMember(2)] public uint AccountType { get; set; }  | not used currently
    
    // [ProtoMember(3)] public uint CountryCode { get; set; }
}

[ProtoPackable]
internal partial class NTLoginErrorInfo
{
    [ProtoMember(1)] public uint ErrorCode { get; set; }
    
    [ProtoMember(2)] public string TipsTitle { get; set; } = string.Empty;

    [ProtoMember(3)] public string TipsContent { get; set; } = string.Empty;
    
    [ProtoMember(4)] public string JumpWording { get; set; } = string.Empty;
    
    [ProtoMember(5)] public string JumpUrl { get; set; } = string.Empty;
}

[ProtoPackable]
internal partial class NTLoginVersion
{
    [ProtoMember(1)] public Version? A { get; set; }
    
    [ProtoMember(2)] public int AppId { get; set; }
    
    [ProtoMember(3)] public string? AppName { get; set; }
    
    [ProtoMember(4)] public string? Qua { get; set; }
}

[ProtoPackable]
internal partial class NTLoginCookie
{
    [ProtoMember(1)] public string? Cookie { get; set; }
}

[ProtoPackable]
internal partial class NTLoginForwardRequest
{
    [ProtoMember(1)] public byte[]? SessionTicket { get; set; }
    
    [ProtoMember(3)] public byte[]? Buffer { get; set; } // pb_buf
    
    [ProtoMember(4)] public int Type { get; set; }
}