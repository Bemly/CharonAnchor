namespace Lagrange.Milky.Configuration;

public class CacheConfiguration
{
    public string Policy { get; set; } = "LRU";

    public int Capacity { get; set; } = 1000;
}