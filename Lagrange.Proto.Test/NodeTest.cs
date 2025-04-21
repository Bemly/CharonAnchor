using Lagrange.Proto.Nodes;
using Lagrange.Proto.Serialization;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Test;

[TestFixture]
public class NodeTest
{
    private TestNode _obj;
    
    private byte[] _bytes;
    
    [SetUp]
    public void SetUp()
    {
        _obj = new TestNode
        {
            Test1 = 114514
        };

        _bytes = ProtoSerializer.Serialize(_obj);
    }
    
    [Test]
    public void TestParse()
    {
        var node = ProtoObject.Parse(_bytes);
        var test = node[1].GetValue<int>();
        var testArray = node[1].AsArray();
        
        Assert.Multiple(() =>
        {
            Assert.That(test, Is.EqualTo(_obj.Test1));
            Assert.That(testArray, Has.Count.EqualTo(1));
            Assert.That(testArray.Root, Is.SameAs(node));
        });
    }

    [Test]
    public void TestSerialize()
    {
        var node = ProtoObject.Parse(_bytes);
        var bytes = node.Serialize();

        Assert.That(bytes, Is.EqualTo(_bytes));
    }

    [Test]
    public void TestConstructor()
    {
        var node = new ProtoObject
        {
            { 1, new ProtoObject { { 1, 2 } } }, 
            { 1, new ProtoObject { { 1, 2 } } }, 
            { 3, 4 },
            { 5, 6 }
        };

        var bytes = node.Serialize();
        Console.WriteLine(Convert.ToHexString(bytes));
        var parsed = ProtoObject.Parse(bytes);

        int value = parsed[1][0][1].GetValue<int>();

        Assert.That(value, Is.EqualTo(2));
    }

    [Test]
    public void TestSerializeToWriter()
    {
        var node = new ProtoObject
        {
            { 1, new ProtoObject { { 1, 2 } } }, { 1, new ProtoObject { { 1, 2 } } }, { 3, 4 }, { 5, 6 }
        };

        var bufferWriter = new SegmentBufferWriter(1024);
        node.Serialize(bufferWriter);

        var bytes = bufferWriter.ToArray();
        var normalBytes = node.Serialize();
        
        Assert.That(bytes, Is.EqualTo(normalBytes));
    }
    
    [Test]
    public void TestHybrid()
    {
        var node = new TestNodeHybrid
        {
            Test1 = 114514,
            Test2 = new ProtoArray(WireType.VarInt, 1, 2, 3, 4, 5, 6, 7, 8, 9),
            Test3 = new ProtoObject
            {
                { 1, 2 }, { 3, 4 }
            }
        };

        var bytes = ProtoSerializer.SerializeProtoPackable(node);
        var parsed = ProtoSerializer.DeserializeProtoPackable<TestNodeHybrid>(bytes);

        Assert.Multiple(() =>
        {
            Assert.That(parsed.Test1, Is.EqualTo(node.Test1));
            Assert.That(parsed.Test2.GetValues<int>(), Is.EqualTo(node.Test2.GetValues<int>()));
            Assert.That(parsed.Test3[1].GetValue<int>(), Is.EqualTo(2));
            Assert.That(parsed.Test3[3].GetValue<int>(), Is.EqualTo(4));
        });
    }
}

[ProtoPackable]
public partial class TestNode
{
    [ProtoMember(1)] public int Test1 { get; set; }
}

[ProtoPackable]
public partial class TestNodeHybrid
{
    [ProtoMember(1)] public int Test1 { get; set; }
    
    [ProtoMember(2, NodesWireType = WireType.VarInt)] public ProtoArray Test2 { get; set; }
    
    [ProtoMember(3)] public ProtoObject Test3 { get; set; }
}