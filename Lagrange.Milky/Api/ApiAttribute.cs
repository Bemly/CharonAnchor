namespace Lagrange.Milky.Api;

[AttributeUsage(AttributeTargets.Class)]
public class ApiAttribute(string name, bool debug = false) : Attribute
{
    public string Name { get; } = name;

    public bool Debug { get; } = debug;
}