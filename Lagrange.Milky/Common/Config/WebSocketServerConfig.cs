namespace Lagrange.Milky.Common.Config;

public class WebSocketServerConfig
{
    public string Host { get; set; } = "";

    public uint Port { get; set; }

    public string? AccessToken { get; set; }
}