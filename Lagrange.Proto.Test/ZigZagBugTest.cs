using System;
using System.Buffers;
using Lagrange.Proto.Primitives;
using Lagrange.Proto.Utility;
using Lagrange.Proto.Serialization;
using NUnit.Framework;

namespace Lagrange.Proto.Test;

[TestFixture]
public class ZigZagBugTest
{
    [Test]
    public void TestSByteZigZagIssue()
    {
        sbyte original = sbyte.MinValue; // -128
        Console.WriteLine($"Original: {original}");
        
        // ZigZag encode
        var encoded = ProtoHelper.ZigZagEncode(original);
        Console.WriteLine($"ZigZag Encoded (sbyte): {encoded}");
        Console.WriteLine($"ZigZag Encoded (as byte): {(byte)encoded}");
        Console.WriteLine($"ZigZag Encoded (as uint): {(uint)(byte)encoded}");
        
        // Write to buffer - the issue might be here
        var buffer = new ArrayBufferWriter<byte>();
        var writer = new ProtoWriter(buffer);
        
        // When we write, we need to make sure it's treated as unsigned
        // The problem is that EncodeVarInt might be treating the sbyte value incorrectly
        writer.EncodeVarInt(encoded);
        writer.Flush();
        
        Console.WriteLine($"Written bytes: {BitConverter.ToString(buffer.WrittenMemory.ToArray())}");
        
        // Now read it back
        var reader = new ProtoReader(buffer.WrittenMemory.Span);
        var readValue = reader.DecodeVarInt<sbyte>();
        Console.WriteLine($"Read value: {readValue}");
        
        // Decode
        var decoded = ProtoHelper.ZigZagDecode(readValue);
        Console.WriteLine($"Decoded: {decoded}");
        
        // The issue is that when encoding -128:
        // 1. ZigZag encode: (-128 << 1) ^ (-128 >> 31) = -256 ^ -1 = 255
        // 2. As sbyte, 255 becomes -1
        // 3. When writing -1 as varint, it writes as a negative number (lots of FF bytes)
        // 4. This causes the read to fail or return wrong value
        
        // The fix: ensure signed values are cast to unsigned before writing
        Console.WriteLine("\n--- Testing with unsigned cast ---");
        buffer.Clear();
        writer = new ProtoWriter(buffer);
        
        // Cast to byte (unsigned) before writing
        byte unsignedEncoded = (byte)encoded;
        writer.EncodeVarInt(unsignedEncoded);
        writer.Flush();
        
        Console.WriteLine($"Written bytes (unsigned): {BitConverter.ToString(buffer.WrittenMemory.ToArray())}");
        
        reader = new ProtoReader(buffer.WrittenMemory.Span);
        byte readUnsigned = reader.DecodeVarInt<byte>();
        sbyte readSigned = (sbyte)readUnsigned;
        Console.WriteLine($"Read value (unsigned): {readUnsigned}");
        Console.WriteLine($"Read value (as signed): {readSigned}");
        
        decoded = ProtoHelper.ZigZagDecode(readSigned);
        Console.WriteLine($"Decoded: {decoded}");
        
        Assert.That(decoded, Is.EqualTo(original));
    }
}