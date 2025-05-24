using Lagrange.Core.Common;

namespace Lagrange.Milky.Core.Configuration;

public class ProtocolConfiguration
{
    public Protocols? Platform { get; set; }

    public SignerConfiguration Signer { get; set; } = new();
}
