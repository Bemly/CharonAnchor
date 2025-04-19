using System.Buffers;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Runner;

internal static partial class Program
{
    private static void Main(string[] args)
    {
        var test = new Test
        {
            Test1 = 1,
            Test2 = 2,
            Test3 = [1, 2, 3],
            Test4 = [1, 2, 3],
            Test5 = TestEnum.Test1,
            Test6 = new Test_2
            {
                Test1 = 1,
                Test2 = 2,
                Test3 = [1, 2, 3],
                Test4 = [1, 2, 3],
                Test5 = TestEnum.Test1
            },
            Test7 = 0.5f,
            Test8 = 0.5d,
            Test9 = [],
            Test10 = "test",
            Test11 = "test"
        };

        var buffer = ProtoSerializer.SerializeProtoPackable(test);
        Console.WriteLine(Convert.ToHexString(buffer));
    }
    
    [ProtoPackable]
    public partial class Test
    {
        [ProtoMember(1)] public int Test1 { get; set; }
        
        [ProtoMember(2)] public int? Test2 { get; set; }
        
        [ProtoMember(3)] public int[] Test3 { get; set; } = [];
        
        [ProtoMember(4)] public List<int> Test4 { get; set; } = [];
        
        [ProtoMember(5)] public TestEnum Test5 { get; set; }
        
        [ProtoMember(6)] public Test_2 Test6 { get; set; } = new();
        
        [ProtoMember(7)] public float Test7 { get; set; }
        
        [ProtoMember(8)] public double Test8 { get; set; }
        
        [ProtoMember(9)] public byte[] Test9 { get; set; } = [];
        
        [ProtoMember(10)] public string Test10 { get; set; } = "";
        
        [ProtoMember(114514)] public string Test11 { get; set; } = "";
    }
    
    [ProtoPackable]
    public partial class Test_2
    {
        [ProtoMember(1)] public int Test1 { get; set; }
        
        [ProtoMember(2)] public int? Test2 { get; set; }
        
        [ProtoMember(3)] public int[] Test3 { get; set; } = [];
        
        [ProtoMember(4)] public List<int> Test4 { get; set; } = [];
        
        [ProtoMember(5)] public TestEnum Test5 { get; set; }
    }
}

public enum TestEnum
{
    Test1 = 1,
    Test2 = 2,
    Test3 = 3,
    Test4 = 4,
}