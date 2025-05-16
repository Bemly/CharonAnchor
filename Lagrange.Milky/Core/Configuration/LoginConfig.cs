namespace Lagrange.Milky.Core.Configuration;

public class LoginConfiguration
{
    public ulong Uin { get; set; }

    public string? Password { get; set; }

    public bool AutoReconnect { get; set; } = true;

    public bool AutoReLogin { get; set; } = true;

    public bool QrCodeConsoleCompatibilityMode { get; set; } = false;

    public bool UseAutoCaptchaResolver { get; set; } = true;
}