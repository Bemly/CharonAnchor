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
}

[ProtoPackable]
public partial class TestNode
{
    [ProtoMember(1)] public int Test1 { get; set; }
}