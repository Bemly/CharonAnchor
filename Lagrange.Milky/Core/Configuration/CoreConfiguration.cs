namespace Lagrange.Milky.Core.Configuration;

public class CoreConfiguration
{
    public ServerConfiguration Server { get; set; } = new();

    public ProtocolConfiguration Protocol { get; set; } = new();

    public LoginConfiguration Login { get; set; } = new();
}
