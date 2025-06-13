namespace Lagrange.Milky.Configuration;

public class CoreConfiguration
{
    public ServerConfiguration Server { get; set; } = new();

    public SignerConfiguration Signer { get; set; } = new();

    public LoginConfiguration Login { get; set; } = new();
}
