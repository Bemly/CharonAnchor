namespace Lagrange.Milky.Implementation.Apis;

[AttributeUsage(AttributeTargets.Class)]
public class ApiAttribute(string api) : Attribute
{
    public string Api { get; } = api;
}