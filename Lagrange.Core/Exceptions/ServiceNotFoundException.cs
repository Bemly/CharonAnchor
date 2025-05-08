namespace Lagrange.Core.Exceptions;

internal class ServiceNotFoundException(string command) : LagrangeException($"Service not found for command: {command}")
{
    public string Command { get; } = command;
}