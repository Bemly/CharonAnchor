using System.Buffers;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Runner;

internal static class Program
{
    private static void Main(string[] args)
    {
        var bufferWriter = new ArrayBufferWriter<byte>();
        
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
                Test8 = TestEnum.Test8
            }
        };

        ProtoSerializer.SerializeProtoPackable(bufferWriter, test);
        var bytes = bufferWriter.WrittenSpan.ToArray();
        var test2 = ProtoSerializer.DeserializeProtoPackable<Test>(bytes);
        
        Console.WriteLine(Convert.ToHexString(bytes));
        Console.WriteLine(test2.Test1);
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


[ProtoPackable(IgnoreDefaultFields = true)]
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
    
    [ProtoMember(8)] public TestEnum Test8 { get; set; } = TestEnum.Test1;
}

public enum TestEnum
{
    Test1 = 1,
    Test2 = 2,
    Test3 = 3,
    Test4 = 4,
    Test5 = 5,
    Test6 = 6,
    Test7 = 7,
    Test8 = 8,
}