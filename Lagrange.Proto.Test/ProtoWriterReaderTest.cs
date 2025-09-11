using System.Buffers;
using System.Runtime.Intrinsics.X86;
using System.Text;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Test;

[TestFixture]
public class ProtoWriterReaderTest
{
    #region VarInt Tests
    
    [Test]
    public void TestVarInt_SingleByte()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(0);
        writer.EncodeVarInt(127);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int value1 = reader.DecodeVarInt<int>();
        int value2 = reader.DecodeVarInt<int>();
        
        bool isCompleted = reader.IsCompleted;
        
        Assert.Multiple(() =>
        {
            Assert.That(value1, Is.EqualTo(0));
            Assert.That(value2, Is.EqualTo(127));
            Assert.That(isCompleted, Is.True);
        });
    }
    
    [Test]
    public void TestVarInt_MultiByte()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(128);
        writer.EncodeVarInt(16383);
        writer.EncodeVarInt(16384);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int value1 = reader.DecodeVarInt<int>();
        int value2 = reader.DecodeVarInt<int>();
        int value3 = reader.DecodeVarInt<int>();
        
        Assert.Multiple(() =>
        {
            Assert.That(value1, Is.EqualTo(128));
            Assert.That(value2, Is.EqualTo(16383));
            Assert.That(value3, Is.EqualTo(16384));
        });
    }
    
    [Test]
    public void TestVarInt_MaxValues()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(byte.MaxValue);
        writer.EncodeVarInt(ushort.MaxValue);
        writer.EncodeVarInt(uint.MaxValue);
        writer.EncodeVarInt(ulong.MaxValue);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        byte byteVal = reader.DecodeVarInt<byte>();
        ushort ushortVal = reader.DecodeVarInt<ushort>();
        uint uintVal = reader.DecodeVarInt<uint>();
        ulong ulongVal = reader.DecodeVarInt<ulong>();
        
        Assert.Multiple(() =>
        {
            Assert.That(byteVal, Is.EqualTo(byte.MaxValue));
            Assert.That(ushortVal, Is.EqualTo(ushort.MaxValue));
            Assert.That(uintVal, Is.EqualTo(uint.MaxValue));
            Assert.That(ulongVal, Is.EqualTo(ulong.MaxValue));
        });
    }
    
    [Test]
    public void TestVarInt_NegativeNumbers()
    {
        // Negative numbers are encoded as unsigned in protobuf
        // They use zigzag encoding when NumberHandling.Signed is specified
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        // Write as unsigned (two's complement)
        writer.EncodeVarInt(unchecked((uint)-1));
        writer.EncodeVarInt(unchecked((uint)-128));
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        uint negOne = reader.DecodeVarInt<uint>();
        uint neg128 = reader.DecodeVarInt<uint>();
        
        Assert.Multiple(() =>
        {
            Assert.That(negOne, Is.EqualTo(uint.MaxValue));
            Assert.That(neg128, Is.EqualTo(uint.MaxValue - 127));
        });
    }
    
    [Test]
    public void TestDecodeVarIntUnsafe_DualValues()
    {
        if (!Sse3.IsSupported)
        {
            Assert.Ignore("SSE3 is not supported on this platform.");
            return;
        }

        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(42);
        writer.EncodeVarInt(123456);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        var (val1, val2) = reader.DecodeVarIntUnsafe<int, int>(buffer.WrittenMemory.Span);
        
        Assert.Multiple(() =>
        {
            Assert.That(val1, Is.EqualTo(42));
            Assert.That(val2, Is.EqualTo(123456));
        });
    }
    
    [Test]
    public void TestDecodeVarIntUnsafe_MixedTypes()
    {
        if (!Sse3.IsSupported)
        {
            Assert.Ignore("SSE3 is not supported on this platform.");
            return;
        }

        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(1145141919810L);
        writer.EncodeVarInt(114514);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        var (val1, val2) = reader.DecodeVarIntUnsafe<long, int>(buffer.WrittenMemory.Span);
        
        Assert.Multiple(() =>
        {
            Assert.That(val1, Is.EqualTo(1145141919810L));
            Assert.That(val2, Is.EqualTo(114514));
        });
    }
    
    #endregion
    
    #region Fixed32/Fixed64 Tests
    
    [Test]
    public void TestFixed32_AllTypes()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeFixed32(42);
        writer.EncodeFixed32(3.14f);
        writer.EncodeFixed32(uint.MaxValue);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int intVal = reader.DecodeFixed32<int>();
        float floatVal = reader.DecodeFixed32<float>();
        uint uintVal = reader.DecodeFixed32<uint>();
        
        Assert.Multiple(() =>
        {
            Assert.That(intVal, Is.EqualTo(42));
            Assert.That(floatVal, Is.EqualTo(3.14f).Within(0.001f));
            Assert.That(uintVal, Is.EqualTo(uint.MaxValue));
        });
    }
    
    [Test]
    public void TestFixed64_AllTypes()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeFixed64(42L);
        writer.EncodeFixed64(3.14159265359);
        writer.EncodeFixed64(ulong.MaxValue);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        long longVal = reader.DecodeFixed64<long>();
        double doubleVal = reader.DecodeFixed64<double>();
        ulong ulongVal = reader.DecodeFixed64<ulong>();
        
        Assert.Multiple(() =>
        {
            Assert.That(longVal, Is.EqualTo(42L));
            Assert.That(doubleVal, Is.EqualTo(3.14159265359).Within(0.000001));
            Assert.That(ulongVal, Is.EqualTo(ulong.MaxValue));
        });
    }
    
    #endregion
    
    #region String Tests
    
    [Test]
    public void TestString_Empty()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeString("");
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int length = reader.DecodeVarInt<int>();
        
        Assert.That(length, Is.EqualTo(0));
    }
    
    [Test]
    public void TestString_Ascii()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeString("Hello, World!");
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        string result = Encoding.UTF8.GetString(span);
        
        bool isCompleted = reader.IsCompleted;
        
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("Hello, World!"));
            Assert.That(isCompleted, Is.True);
        });
    }
    
    [Test]
    public void TestString_Unicode()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        string unicodeStr = "你好世界 🌍 Émoji 测试";
        writer.EncodeString(unicodeStr);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        string result = Encoding.UTF8.GetString(span);
        
        Assert.That(result, Is.EqualTo(unicodeStr));
    }
    
    [Test]
    public void TestString_LongString()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        string longStr = new string('A', 10000);
        writer.EncodeString(longStr);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        string result = Encoding.UTF8.GetString(span);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.Length, Is.EqualTo(10000));
            Assert.That(result, Is.EqualTo(longStr));
        });
    }
    
    #endregion
    
    #region Bytes Tests
    
    [Test]
    public void TestBytes_Empty()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeBytes(Array.Empty<byte>());
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int length = reader.DecodeVarInt<int>();
        
        Assert.That(length, Is.EqualTo(0));
    }
    
    [Test]
    public void TestBytes_Small()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        byte[] data = { 1, 2, 3, 4, 5 };
        writer.EncodeBytes(data);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        
        byte[] spanArray = span.ToArray();
        
        Assert.Multiple(() =>
        {
            Assert.That(length, Is.EqualTo(5));
            Assert.That(spanArray, Is.EqualTo(data));
        });
    }
    
    [Test]
    public void TestBytes_Large()
    {
        var buffer = new SegmentBufferWriter();
        var writer = new ProtoWriter(buffer);
        
        byte[] largeData = new byte[1024 * 1024]; // 1MB
        Random.Shared.NextBytes(largeData);
        
        writer.EncodeBytes(largeData);
        writer.Flush();
        
        var memory = buffer.CreateReadOnlyMemory();
        var reader = new ProtoReader(memory.Span);
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        
        byte[] spanArray = span.ToArray();
        
        Assert.Multiple(() =>
        {
            Assert.That(length, Is.EqualTo(largeData.Length));
            Assert.That(spanArray, Is.EqualTo(largeData));
        });
        
        buffer.Dispose();
    }
    
    #endregion
    
    #region Raw Byte Operations
    
    [Test]
    public void TestWriteRawByte()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.WriteRawByte(0x00);
        writer.WriteRawByte(0xFF);
        writer.WriteRawByte(0x42);
        writer.Flush();
        
        Assert.That(buffer.WrittenMemory.ToArray(), Is.EqualTo(new byte[] { 0x00, 0xFF, 0x42 }));
    }
    
    [Test]
    public void TestWriteRawBytes()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        byte[] raw = { 0x01, 0x02, 0x03, 0x04 };
        writer.WriteRawBytes(raw);
        writer.Flush();
        
        Assert.That(buffer.WrittenMemory.ToArray(), Is.EqualTo(raw));
    }
    
    #endregion
    
    #region Skip Field Tests
    
    [Test]
    public void TestSkipField_VarInt()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(12345);
        writer.EncodeVarInt(67890);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        reader.SkipField(WireType.VarInt);
        int value = reader.DecodeVarInt<int>();
        
        Assert.That(value, Is.EqualTo(67890));
    }
    
    [Test]
    public void TestSkipField_Fixed32()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeFixed32(3.14f);
        writer.EncodeFixed32(2.71f);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        reader.SkipField(WireType.Fixed32);
        float value = reader.DecodeFixed32<float>();
        
        Assert.That(value, Is.EqualTo(2.71f).Within(0.001f));
    }
    
    [Test]
    public void TestSkipField_Fixed64()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeFixed64(3.14159);
        writer.EncodeFixed64(2.71828);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        reader.SkipField(WireType.Fixed64);
        double value = reader.DecodeFixed64<double>();
        
        Assert.That(value, Is.EqualTo(2.71828).Within(0.00001));
    }
    
    [Test]
    public void TestSkipField_LengthDelimited()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeString("Skip this");
        writer.EncodeString("Read this");
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        reader.SkipField(WireType.LengthDelimited);
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        string value = Encoding.UTF8.GetString(span);
        
        Assert.That(value, Is.EqualTo("Read this"));
    }
    
    #endregion
    
    #region Writer State Tests
    
    [Test]
    public void TestWriter_BytesPending()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        Assert.That(writer.BytesPending, Is.EqualTo(0));
        
        writer.EncodeVarInt(42);
        Assert.That(writer.BytesPending, Is.EqualTo(1));
        
        writer.EncodeVarInt(300);
        Assert.That(writer.BytesPending, Is.EqualTo(3));
        
        writer.Flush();
        Assert.That(writer.BytesPending, Is.EqualTo(0));
        Assert.That(writer.BytesCommitted, Is.EqualTo(3));
    }
    
    [Test]
    public void TestWriter_Reset()
    {
        var buffer1 = new ArrayBufferWriter<byte>();
        var buffer2 = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer1);
        
        writer.EncodeVarInt(42);
        writer.Flush();
        
        writer.Reset(buffer2);
        writer.EncodeVarInt(100);
        writer.Flush();
        
        Assert.Multiple(() =>
        {
            Assert.That(buffer1.WrittenMemory.ToArray(), Is.EqualTo(new byte[] { 42 }));
            Assert.That(buffer2.WrittenMemory.ToArray(), Is.EqualTo(new byte[] { 100 }));
        });
    }
    
    #endregion
    
    #region Reader State Tests
    
    [Test]
    public void TestReader_IsCompleted()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(42);
        writer.EncodeVarInt(100);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        
        bool isCompleted1 = reader.IsCompleted;
        Assert.That(isCompleted1, Is.False);
        
        reader.DecodeVarInt<int>();
        bool isCompleted2 = reader.IsCompleted;
        Assert.That(isCompleted2, Is.False);
        
        reader.DecodeVarInt<int>();
        bool isCompleted3 = reader.IsCompleted;
        Assert.That(isCompleted3, Is.True);
    }
    
    [Test]
    public void TestReader_Rewind()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        writer.EncodeVarInt(42);
        writer.EncodeVarInt(100);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        
        int first = reader.DecodeVarInt<int>();
        reader.Rewind(-1); // Go back one byte
        int second = reader.DecodeVarInt<int>();
        
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(42));
            Assert.That(second, Is.EqualTo(42));
        });
    }
    
    #endregion
    
    #region Edge Cases and Error Handling
    
    [Test]
    public void TestVarInt_MaxBytesPerType()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        // Test maximum bytes needed for each type
        writer.EncodeVarInt((byte)0xFF);           // 2 bytes max for u8
        writer.EncodeVarInt((ushort)0xFFFF);       // 3 bytes max for u16
        writer.EncodeVarInt((uint)0xFFFFFFFF);     // 5 bytes max for u32
        writer.EncodeVarInt((ulong)0xFFFFFFFFFFFFFFFF); // 10 bytes max for u64
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        byte b = reader.DecodeVarInt<byte>();
        ushort us = reader.DecodeVarInt<ushort>();
        uint ui = reader.DecodeVarInt<uint>();
        ulong ul = reader.DecodeVarInt<ulong>();
        
        Assert.Multiple(() =>
        {
            Assert.That(b, Is.EqualTo(0xFF));
            Assert.That(us, Is.EqualTo(0xFFFF));
            Assert.That(ui, Is.EqualTo(0xFFFFFFFF));
            Assert.That(ul, Is.EqualTo(0xFFFFFFFFFFFFFFFF));
        });
    }
    
    [Test]
    public void TestWriter_GrowthBehavior()
    {
        var buffer = new ArrayBufferWriter<byte>(1); // Start with minimal size
        var writer = new ProtoWriter(buffer);
        
        // Force multiple growth operations
        byte[] largeData = new byte[2048];
        Random.Shared.NextBytes(largeData);
        
        writer.EncodeBytes(largeData);
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        int length = reader.DecodeVarInt<int>();
        var span = reader.CreateSpan(length);
        
        Assert.That(span.ToArray(), Is.EqualTo(largeData));
    }
    
    [Test]
    public void TestReader_InsufficientData()
    {
        byte[] data = { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }; // Malformed varint
        
        Assert.Throws<InvalidDataException>(() => 
        {
            var reader = new ProtoReader(data);
            reader.DecodeVarInt<int>();
        });
    }
    
    [Test]
    public void TestReader_InsufficientDataForFixed32()
    {
        byte[] data = { 0x01, 0x02, 0x03 }; // Only 3 bytes, need 4
        
        Assert.Throws<ArgumentOutOfRangeException>(() => 
        {
            var reader = new ProtoReader(data);
            reader.DecodeFixed32<int>();
        });
    }
    
    [Test]
    public void TestReader_InsufficientDataForFixed64()
    {
        byte[] data = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }; // Only 7 bytes, need 8
        
        Assert.Throws<ArgumentOutOfRangeException>(() => 
        {
            var reader = new ProtoReader(data);
            reader.DecodeFixed64<long>();
        });
    }
    
    #endregion
    
    #region Performance Tests
    
    [Test]
    public void TestPerformance_LargeNumberOfVarInts()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        const int count = 10000;
        for (int i = 0; i < count; i++)
        {
            writer.EncodeVarInt(i);
        }
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        for (int i = 0; i < count; i++)
        {
            int value = reader.DecodeVarInt<int>();
            Assert.That(value, Is.EqualTo(i));
        }
        
        bool isCompleted = reader.IsCompleted;
        Assert.That(isCompleted, Is.True);
    }
    
    [Test]
    public void TestPerformance_MixedOperations()
    {
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        for (int i = 0; i < 10; i++)
        {
            writer.EncodeVarInt(i);
            writer.EncodeFixed32((float)i * 1.5f);
            writer.EncodeBytes(new byte[] { (byte)i, (byte)(i + 1) });
        }
        writer.Flush();
        
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        for (int i = 0; i < 10; i++)
        {
            int intVal = reader.DecodeVarInt<int>();
            float floatVal = reader.DecodeFixed32<float>();
            int bytesLen = reader.DecodeVarInt<int>();
            byte[] bytesVal = reader.CreateSpan(bytesLen).ToArray();
            
            Assert.Multiple(() =>
            {
                Assert.That(intVal, Is.EqualTo(i));
                Assert.That(floatVal, Is.EqualTo((float)i * 1.5f).Within(0.001f));
                Assert.That(bytesVal, Is.EqualTo(new byte[] { (byte)i, (byte)(i + 1) }));
            });
        }
    }
    
    #endregion
}