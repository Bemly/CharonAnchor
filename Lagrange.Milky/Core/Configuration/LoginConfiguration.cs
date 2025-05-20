namespace Lagrange.Milky.Core.Configuration;

public class LoginConfiguration
{
    public uint? Uin { get; set; }

    public string? Password { get; set; }

    public string DeviceName { get; set; } = "LGR-Milky";

    public bool AutoReLogin { get; set; } = true;

    public bool CompatibleQrCode { get; set; } = false;

    public bool UseOnlineCaptchaResolver { get; set; } = true;
}