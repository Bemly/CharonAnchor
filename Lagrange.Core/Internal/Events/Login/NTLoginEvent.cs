using Lagrange.Core.Internal.Services.Login;

namespace Lagrange.Core.Internal.Events.Login;

internal interface INTLoginEventResp
{
    public NTLoginCommon.State State { get; }
    
    public (string, string) Tips { get; }
}

internal class EasyLoginEventReq : ProtocolEvent;

internal class UnusualEasyLoginEventReq : ProtocolEvent;

internal class PasswordLoginEventReq(string password, (string, string, string)? captcha) : ProtocolEvent
{
    public string Password { get; } = password;

    public (string, string, string)? Captcha { get; } = captcha;
}

internal class PasswordLoginEventResp(NTLoginCommon.State state, (string, string)? tips, string? jumpingUrl) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginCommon.State State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);

    public string JumpingUrl { get; } = jumpingUrl ?? string.Empty;
}

internal class EasyLoginEventResp(NTLoginCommon.State state, (string, string)? tips, byte[]? unusualSigs) : ProtocolEvent, INTLoginEventResp
{
    public NTLoginCommon.State State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
    
    public byte[]? UnusualSigs { get; } = unusualSigs;
}

internal class UnusualEasyLoginEventResp(NTLoginCommon.State state, (string, string)? tips) : ProtocolEvent
{
    public NTLoginCommon.State State { get; } = state;

    public (string, string) Tips { get; } = tips ?? (string.Empty, string.Empty);
}