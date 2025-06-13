namespace Lagrange.Milky.Configuration;

public class MilkyConfiguration
{
    public string? Host { get; set; }

    public ulong? Port { get; set; }

    public string Prefix { get; set; } = "/";

    public bool UseWebSocket { get; set; } = true;

    public string? AccessToken { get; set; }

    public WebHookConfiguration? WebHook { get; set; }

    public bool IgnoreBotMessage { get; set; } = false;
}
