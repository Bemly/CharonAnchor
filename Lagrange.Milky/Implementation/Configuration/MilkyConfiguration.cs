namespace Lagrange.Milky.Implementation.Configuration;

public class MilkyConfiguration
{
    public string? Host { get; set; }

    public ulong? Port { get; set; }

    public string ApiPrefix { get; set; } = "/api";

    public string? EventPath { get; set; }

    public string? AccessToken { get; set; }

    public WebHookConfiguration? WebHook { get; set; }
}
