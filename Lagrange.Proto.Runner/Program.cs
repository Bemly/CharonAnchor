using System.Buffers;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Runner;

internal static partial class Program
{
    private static void Main(string[] args)
    {
        var test = new Test { Test1 = new Dictionary<int, int> { { 1, 2 }, { 3, 4 }, { 5, 6 } } };

        var buffer = ProtoSerializer.Serialize(test);
        var deserialized = ProtoSerializer.Deserialize<Test>(buffer);
        Console.WriteLine(Convert.ToHexString(buffer));
    }

    public partial class Test
    {
        [ProtoMember(1)] [ProtoValueMember]
        public Dictionary<int, int> Test1 { get; set; } = new();
    }
}