namespace Lagrange.Milky.Configuration;

public class MilkyConfiguration
{
    public string? Host { get; set; }

    public ulong? Port { get; set; }

    public string Prefix { get; set; } = "/";

    public string? AccessToken { get; set; }

    public bool EnabledWebSocket { get; set; } = true;

    public WebHookConfiguration? WebHook { get; set; }

    public MessageConfiguration Message { get; set; } = new MessageConfiguration();

    public bool Debug { get; set; } = false;
}
