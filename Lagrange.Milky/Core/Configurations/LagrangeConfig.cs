namespace Lagrange.Milky.Core.Configurations;

public class LagrangeConfiguration
{
    public ProtocolConfiguration Protocol { get; set; } = new ProtocolConfiguration();

    public LoginConfiguration Login { get; set; } = new LoginConfiguration();
}