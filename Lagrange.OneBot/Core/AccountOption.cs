using Lagrange.Core.Common;

namespace Lagrange.OneBot.Core;

[Serializable]
public class AccountOption
{
    public long Uin { get; set; }
    
    public string? Password { get; set; }
    
    public Protocols Protocol { get; set; }
    
    public bool UseIPv6Network { get; set; } = false;
    
    public bool AutoReconnect { get; set; } = true;
    
    public bool GetOptimumServer { get; set; } = true;
    
    public bool AutoReLogin { get; set; } = true;
}