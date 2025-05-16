namespace Lagrange.Milky.Implementation.Api;

[AttributeUsage(AttributeTargets.Class)]
public class ApiAttribute(string api) : Attribute
{
    public string Api { get; } = api;
}