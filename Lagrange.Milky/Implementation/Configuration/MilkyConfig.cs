namespace Lagrange.Milky.Implementation.Configuration;

public class MilkyConfiguration
{
    public string Host { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 8080;

    public string CommonPrefix { get; set; } = "/";

    public string? AccessToken { get; set; } = null;

    public bool IgnoreSelfMessages { get; set; } = false;

    public string? MusicSignServerUrl { get; set; }
}