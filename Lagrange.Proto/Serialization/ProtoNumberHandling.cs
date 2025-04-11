namespace Lagrange.Proto.Serialization;

[Flags]
public enum ProtoNumberHandling : byte
{
    /// <summary>
    /// Variable size and unsigned
    /// </summary>
    Default = 0b0000000,
    
    /// <summary>
    /// Fixed32 Size
    /// </summary>
    Fixed32 = 0b0000001,
    
    /// <summary>
    /// Fixed64 Size
    /// </summary>
    Fixed64 = 0b0000010,
    
    /// <summary>
    /// Signed VarInt
    /// </summary>
    Signed = 0b0000100
}