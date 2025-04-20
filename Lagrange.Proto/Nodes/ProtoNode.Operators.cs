namespace Lagrange.Proto.Nodes;

public partial class ProtoNode
{
    public static implicit operator ProtoNode(bool value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(sbyte value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(byte value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(short value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(ushort value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(int value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(uint value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(long value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(ulong value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(float value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(double value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(bool? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(sbyte? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(byte? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(short? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(ushort? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(int? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(uint? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(long? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(ulong? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(float? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(double? value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(string value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(byte[] value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(ReadOnlyMemory<char> value) => ProtoValue.Create(value);
    
    public static implicit operator ProtoNode(ReadOnlyMemory<byte> value) => ProtoValue.Create(value);
}