using Lagrange.Core.Internal.Packets.Struct;

namespace Lagrange.Core.Internal.Services;

[AttributeUsage(AttributeTargets.Class)]
internal class ServiceAttribute(
    string command, 
    RequestType requestType = RequestType.D2Auth,
    EncryptType encryptType = EncryptType.EncryptD2Key) : Attribute
{
    public string Command { get; } = command;
    
    public RequestType RequestType { get; } = requestType;
    
    public EncryptType EncryptType { get; } = encryptType;
    
    public bool DisableLog { get; init; } = false;
}