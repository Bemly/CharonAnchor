namespace Lagrange.Milky.Common.Config;

public class HttpClientConfig
{
    public string Host { get; set; } = "";

    public uint Port { get; set; }

    public string Suffix { get; set; } = "";

    public string? AccessToken { get; set; }
}