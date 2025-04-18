using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Test;

[TestFixture]
public class CollectionTest
{
    private TestCollection _test;
    
    [SetUp]
    public void Setup()
    {
        _test = new TestCollection
        {
            Test1 = new[] { 1, 2, 3 },
            Test2 = new[] { "Test", "Test2" },
            Test3 = new[] { 1.0f, 2.0f, 3.0f }
        };
    }

    [Test]
    public void TestSerializeProtoPackable()
    {
        var bytes = ProtoSerializer.SerializeProtoPackable(_test);
        var test2 = ProtoSerializer.DeserializeProtoPackable<TestCollection>(bytes);

        Assert.Multiple(() =>
        {
            Assert.That(test2.Test1, Is.EqualTo(_test.Test1));
            Assert.That(test2.Test2, Is.EqualTo(_test.Test2));
            Assert.That(test2.Test3, Is.EqualTo(_test.Test3));
        });
    }
    
    [Test]
    public void TestSerializeProto()
    {
        var bytes = ProtoSerializer.Serialize(_test);
        var test2 = ProtoSerializer.Deserialize<TestCollection>(bytes);

        Assert.Multiple(() =>
        {
            Assert.That(test2.Test1, Is.EqualTo(_test.Test1));
            Assert.That(test2.Test2, Is.EqualTo(_test.Test2));
            Assert.That(test2.Test3, Is.EqualTo(_test.Test3));
        });
    }
}

[ProtoPackable]
public partial class TestCollection
{
    [ProtoMember(1)] public int[] Test1 { get; set; } = [];
    
    [ProtoMember(2)] public string[] Test2 { get; set; } = [];
    
    [ProtoMember(3)] public float[] Test3 { get; set; } = [];
}