using System;
using NUnit.Framework;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Test;

[TestFixture]
public class SimpleZigZagTest
{
    [Test]
    public void TestZigZagEncodingLogic()
    {
        // Test sbyte values
        sbyte minSByte = sbyte.MinValue; // -128
        Console.WriteLine($"Original sbyte: {minSByte}");
        
        // Manual zigzag encode for sbyte
        int v = (int)minSByte;
        Console.WriteLine($"As int: {v}");
        
        int encoded = (v << 1) ^ (v >> 31);
        Console.WriteLine($"Encoded as int: {encoded}");
        
        sbyte encodedSByte = (sbyte)encoded;
        Console.WriteLine($"Encoded as sbyte: {encodedSByte}");
        
        // Using the helper
        var helperEncoded = ProtoHelper.ZigZagEncode(minSByte);
        Console.WriteLine($"Helper encoded: {helperEncoded}");
        
        // Test with -1
        sbyte negOne = -1;
        Console.WriteLine($"\nOriginal sbyte: {negOne}");
        v = (int)negOne;
        Console.WriteLine($"As int: {v}");
        encoded = (v << 1) ^ (v >> 31);
        Console.WriteLine($"Encoded as int: {encoded}");
        encodedSByte = (sbyte)encoded;
        Console.WriteLine($"Encoded as sbyte: {encodedSByte}");
        
        // Test sbyte max
        sbyte maxSByte = sbyte.MaxValue; // 127
        Console.WriteLine($"\nOriginal sbyte: {maxSByte}");
        v = (int)maxSByte;
        Console.WriteLine($"As int: {v}");
        encoded = (v << 1) ^ (v >> 31);
        Console.WriteLine($"Encoded as int: {encoded}");
        encodedSByte = (sbyte)encoded;
        Console.WriteLine($"Encoded as sbyte: {encodedSByte}");
        helperEncoded = ProtoHelper.ZigZagEncode(maxSByte);
        Console.WriteLine($"Helper encoded: {helperEncoded}");
        
        // Now test decoding
        Console.WriteLine("\n--- DECODING ---");
        
        // Decode -1 (which was encoded from -128)
        sbyte encodedValue = -1;
        Console.WriteLine($"Encoded value to decode: {encodedValue}");
        
        int asInt = (int)encodedValue; // This will be -1 due to sign extension
        Console.WriteLine($"As int (sign extended): {asInt}");
        
        // But we need to treat it as unsigned for zigzag decode
        int unsignedInt = encodedValue & 0xFF; // Treat as unsigned byte
        Console.WriteLine($"As unsigned int: {unsignedInt}");
        
        int decoded = (unsignedInt >> 1) ^ -(unsignedInt & 1);
        Console.WriteLine($"Decoded: {decoded}");
        sbyte decodedSByte = (sbyte)decoded;
        Console.WriteLine($"Decoded as sbyte: {decodedSByte}");
        
        // Using helper
        var helperDecoded = ProtoHelper.ZigZagDecode(encodedValue);
        Console.WriteLine($"Helper decoded: {helperDecoded}");
    }
}