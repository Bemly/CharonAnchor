namespace Lagrange.Core.Internal.Events.Login;

internal class LoginEventReq(LoginEventReq.Command cmd) : ProtocolEvent
{
    public enum Command
    {
        Tgtgt = 0x09
    }
    
    public Command Cmd { get; } = cmd;
}

internal class LoginEventResp : ProtocolEvent
{
    public byte RetCode { get; }
    
    public (string, string)? Error { get; }
    
    public Dictionary<ushort, byte[]> Tlvs { get; set; }

    public LoginEventResp(byte retCode, (string, string) error)
    {
        RetCode = retCode;
        Error = error;
        Tlvs = new Dictionary<ushort, byte[]>();
    }
    
    public LoginEventResp(byte retCode, Dictionary<ushort, byte[]> tlvs)
    {
        RetCode = retCode;
        Tlvs = tlvs;
    }
}