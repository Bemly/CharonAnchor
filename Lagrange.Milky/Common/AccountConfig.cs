using Lagrange.Core.Common;

namespace Lagrange.Milky.Common;

public class AccountConfig
{
    public Protocols Protocol { get; set; }

    public long Uin { get; set; }

    public string? Password { get; set; }

    public bool UseIPv6Network { get; set; } = false;

    public bool AutoReconnect { get; set; } = true;

    public bool GetOptimumServer { get; set; } = true;

    public bool AutoReLogin { get; set; } = true;
}