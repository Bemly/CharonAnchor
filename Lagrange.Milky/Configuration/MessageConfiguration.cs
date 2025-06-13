namespace Lagrange.Milky.Configuration;

public class MessageConfiguration
{
    public bool IgnoreBotMessage { get; set; } = false;

    public CacheConfiguration Cache { get; set; } = new CacheConfiguration();
}
