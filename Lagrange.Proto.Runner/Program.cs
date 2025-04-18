using System.Buffers;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Runner;

internal static class Program
{
    private static void Main(string[] args)
    {
        var test = new Test
        {
            Test1 = 1,
            Test2 = "Test",
            Test3 = 3.14f,
            Test4 = 3.14,
            Test5 = 5,
            Test6 = "Test6",
            Test7 = [1, 2, 3],
            Test8 = [1, 2, 3, 4, 5],
            Test9 = new byte[] { 1, 2, 3 },
            Test10 = new byte[] { 1, 2, 3 },
            Test11 = new char[] { '1', '2', '3' },
            Test12 = new char[] { '1', '2', '3' },
            Test13 = true,
            Test14 = [1, 2, 3, 4, 5]
        };

        var bytes = ProtoSerializer.SerializeProtoPackable(test);
        var test2 = ProtoSerializer.DeserializeProtoPackable<Test>(bytes);
        
        Console.WriteLine(Convert.ToHexString(bytes));
        Console.WriteLine(test2.Test1);
    }
}

[ProtoPackable]
public partial class Test
{
    [ProtoMember(1, NumberHandling = ProtoNumberHandling.Signed)] 
    public int Test1 { get; set; }
        
    [ProtoMember(2)] public string Test2 { get; set; } = string.Empty;
    
    [ProtoMember(3)] public float Test3 { get; set; }

    [ProtoMember(4)] public double Test4;
    
    [ProtoMember(5)] public int? Test5 { get; set; }
    
    [ProtoMember(6)] public string? Test6 { get; set; }
    
    [ProtoMember(7)] public byte[] Test7 { get; set; } = [];
    
    [ProtoMember(8)] public int[] Test8 { get; set; } = [];
    
    [ProtoMember(9)] public ReadOnlyMemory<byte> Test9 { get; set; } = Array.Empty<byte>();
    
    [ProtoMember(10)] public Memory<byte> Test10 { get; set; } = Array.Empty<byte>();
    
    [ProtoMember(11)] public ReadOnlyMemory<char> Test11 { get; set; } = Array.Empty<char>();
    
    [ProtoMember(12)] public Memory<char> Test12 { get; set; } = Array.Empty<char>();
    
    [ProtoMember(13)] public bool Test13 { get; set; } = true;
    
    [ProtoMember(14)] public List<int> Test14 { get; set; } = [];
}
