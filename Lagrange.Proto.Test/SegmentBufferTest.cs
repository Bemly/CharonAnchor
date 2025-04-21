using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Test;

public class SegmentBufferTest
{
    private SegmentBufferWriter _writer;

    private byte[] _raw;
    
    [SetUp]
    public void Setup()
    {
        _writer = new SegmentBufferWriter();

        _raw = GC.AllocateUninitializedArray<byte>(1024 * 10);
        Random.Shared.NextBytes(_raw);

        var span = _writer.GetSpan(_raw.Length);
        _writer.Advance(_raw.Length);
        
        _raw.AsSpan().CopyTo(span);
    }
    
    [Test]
    public void TestCreateReadeOnlyMemory()
    {
        var reader = _writer.CreateReadOnlyMemory();
        Assert.That(reader.Length, Is.EqualTo(_raw.Length));
        Assert.That(reader.Span.ToArray(), Is.EqualTo(_raw));
    }
    
    [TearDown]
    public void TearDown()
    {
        _writer.Dispose();
    }
}