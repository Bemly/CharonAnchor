namespace Lagrange.Proto;

[AttributeUsage(AttributeTargets.Class)]
public class ProtoPackableAttribute : Attribute
{
    /// <summary>
    /// if true, the fields with default value would be ignored, this would follow the convention of proto3 that
    /// default varint 0 is not serialized.
    /// </summary>
    public bool IgnoreDefaultFields { get; init; }
}