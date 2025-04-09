namespace Lagrange.OneBot.Network.Options;

public class HttpServiceOptions
{
    public string Host { get; set; } = "";

    public uint Port { get; set; }

    public string? AccessToken { get; set; }
}