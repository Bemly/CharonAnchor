using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Login;

#pragma warning disable CS8618 // Non-nullable field is uninitialized.

[ProtoPackable]
internal partial class NTLoginAccount
{
    [ProtoMember(1)] public string Account { get; set; } = string.Empty;
    
    // [ProtoMember(2)] public uint AccountType { get; set; }  | not used currently
    
    // [ProtoMember(3)] public uint CountryCode { get; set; }
}

[ProtoPackable]
internal partial class NTLoginSystem
{
    [ProtoMember(1)] public string? DevType { get; set; }
    
    [ProtoMember(2)] public string? DevName { get; set; }

    [ProtoMember(3)] public int RegisterVendorType { get; set; }
    
    [ProtoMember(4)] public byte[]? Guid { get; set; }
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
    [ProtoMember(1)] public string? Version { get; set; }
    
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
internal partial class NTLoginCredentials
{
    [ProtoMember(3)] public byte[] A1 { get; set; }
    
    [ProtoMember(4)] public byte[] A2 { get; set; }

    [ProtoMember(5)] public byte[] D2 { get; set; }
    
    [ProtoMember(6)] public byte[] D2Key { get; set; }
}

[ProtoPackable]
internal partial class NTLoginCaptcha
{
    [ProtoMember(1)] public string? ProofWaterSig { get; set; }
    
    [ProtoMember(2)] public string? ProofWaterRand { get; set; }
    
    [ProtoMember(3)] public string? ProofWaterSid { get; set; }
}

[ProtoPackable]
internal partial class NTLoginCaptchaResponse
{
    [ProtoMember(3)] public string Url { get; set; }
}

[ProtoPackable]
internal partial class NTLoginUnusualResponse
{
    [ProtoMember(2)] public byte[] Sig { get; set; }
}

[ProtoPackable]
internal partial class NTLoginHead
{
    [ProtoMember(1)] public NTLoginAccount Account { get; set; }
    
    [ProtoMember(2)] public NTLoginSystem System { get; set; }

    [ProtoMember(3)] public NTLoginVersion Version { get; set; }
    
    [ProtoMember(4)] public NTLoginErrorInfo? ErrorInfo { get; set; }
    
    [ProtoMember(5)] public NTLoginCookie? Cookie { get; set; }
}

[ProtoPackable]
internal partial class NTLoginRequest
{
    [ProtoMember(1)] public byte[] Sig { get; set; } // sig can be A1, ClientA1
    
    [ProtoMember(2)] public NTLoginCaptcha? Captcha { get; set; }
}

[ProtoPackable]
internal partial class NTLoginResponse
{
    [ProtoMember(1)] public NTLoginCredentials Credentials { get; set; }
    
    [ProtoMember(2)] public NTLoginCaptchaResponse Captcha { get; set; }
    
    [ProtoMember(3)] public NTLoginUnusualResponse Unusual { get; set; }
}

[ProtoPackable]
internal partial class NTLogin
{
    [ProtoMember(1)] public NTLoginHead Head { get; set; }
    
    [ProtoMember(2)] public ReadOnlyMemory<byte> Body { get; set; }
}

[ProtoPackable]
internal partial class NTLoginForwardRequest
{
    [ProtoMember(1)] public byte[]? SessionTicket { get; set; }
    
    [ProtoMember(3)] public byte[]? Buffer { get; set; } // pb_buf
    
    [ProtoMember(4)] public int Type { get; set; }
}