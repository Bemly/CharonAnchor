using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Runner;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}

[ProtoPackable]
public partial class Test
{
    [ProtoMember(1, NumberHandling = ProtoNumberHandling.Fixed64 | ProtoNumberHandling.Signed)] 
    public int Test1 { get; set; }
        
    [ProtoMember(2)] public string Test2 { get; set; } = string.Empty;
    
    [ProtoMember(3)] public float Test3 { get; set; }
    
    [ProtoMember(4)] public double Test4 { get; set; }
    
    [ProtoMember(5)] public int? Test5 { get; set; }
    
    [ProtoMember(6)] public string? Test6 { get; set; }
    
    [ProtoMember(7)] public byte[] Test7 { get; set; } = [];
}