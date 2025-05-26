namespace Lagrange.Milky.Implementation.Configuration;

public class WebHookConfiguration
{
    public string? Host { get; set; }

    public ulong? Port { get; set; }

    public string Path { get; set; } = "/webhook";
}