namespace Lagrange.Milky.Implementation.Configuration;

public class WebSocketConfiguration
{
    public string? Host { get; set; }

    public ulong? Port { get; set; }

    public string Prefix { get; set; } = "/";

    public string? AccessToken { get; set; }
}