using System.Runtime.CompilerServices;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Runner;

internal static class Program
{
    private static void Main(string[] args)
    {
        var bufferWriter = new SegmentBufferWriter();
        
        var writer = new ProtoWriter(bufferWriter);
        var test = new Test
        {
            Test1 = 1,
            Test2 = "Test",
            Test3 = 3.14f,
            Test4 = 3.14,
            Test5 = 5,
            Test6 = "Test6",
            Test7 = [1, 2, 3],
            Test8 = new Test_2
            {
                Test1 = 1,
                Test2 = "Test",
                Test3 = 3.14f,
                Test4 = 3.14,
                Test5 = 5,
                Test6 = "Test6",
                Test7 = [1, 2, 3],
            }
        };
        
        Test.SerializeHandler(test, writer);
        writer.Flush();
        
        Console.WriteLine(Convert.ToHexString(bufferWriter.CreateReadOnlyMemory().Span));
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
    
    [ProtoMember(8)] public Test_2 Test8 { get; set; } = new();
}


[ProtoPackable]
public partial class Test_2
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