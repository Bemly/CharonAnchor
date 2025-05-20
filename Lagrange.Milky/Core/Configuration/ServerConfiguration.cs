namespace Lagrange.Milky.Core.Configuration;

public class ServerConfiguration
{
    public bool AutoReconnect { get; set; } = true;

    public bool UseIPv6Network { get; set; } = false;

    public bool GetOptimumServer { get; set; } = true;
}