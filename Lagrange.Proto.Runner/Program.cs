using System.Buffers;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Runner;

internal static partial class Program
{
    private static void Main(string[] args)
    {
        var test = new Test { Test1 = new Dictionary<int, int> { { 1, 2 }, { 3, 4 }, { 5, 6 } } };

        var buffer = ProtoSerializer.SerializeProtoPackable(test);
        Console.WriteLine(Convert.ToHexString(buffer));
    }

    [ProtoPackable]
    public partial class Test
    {
        [ProtoMember(1)] [ProtoValueMember(NumberHandling = ProtoNumberHandling.Signed)]
        public Dictionary<int, int> Test1 { get; set; } = new Dictionary<int, int>();
    }
}