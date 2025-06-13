namespace Lagrange.Milky.Api;

[AttributeUsage(AttributeTargets.Class)]
public class ApiAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}