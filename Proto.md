## Lagrange.Proto

### The Blazing-fast Code-first C# Google Protocol Buffer Serializer

### Overview

 + Low memory allocation
 + AOT Friendly using Roslyn's Source Generators
 + Extreme-fast Serialization
 + x86-64 SIMD Support for the VarInt Encoding
 + Supported C# Type
   + string, byte[], ReadOnlyMemory<byte> (Length-Delimited)
   + bool, sbyte, byte, short, ushort, int, uint, long, ulong (VarInt)
   + float, double (Fixed32, Fixed64)
   + ProtoPackable (nested object for Length-Delimited)
   + List\<T>, T[], ReadOnlyCollection\<T> (repeated field)
   + IDictionary\<TKey, TValue> (map field)
   + Nullable\<T> (optional field)
   + Enum with valid underlying type (VarInt)

### Usage
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[ProtoPackable]
public class Person
{
    [ProtoMember(1)] public string Name { get; set; }
   
    [ProtoMember(2)] public int Age { get; set; }
    
    [ProtoMember(3)] public List<string> Hobbies { get; set; }
}
```

After defining the class, you can serialize and deserialize it using the `ProtoSerializer` class.

```csharp
var result = ProtoSerializer.Serialize(new Person
{
    Name = "John Doe",
    Age = 30,
    Hobbies = new List<string> { "Reading", "Traveling" }
});
Console.WriteLine($"Serialized: {BitConverter.ToString(resutl)}");
var person = ProtoSerializer.Deserialize<Person>(result.AsSpan());
Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
Console.WriteLine($"Hobbies: {string.Join(", ", person.Hobbies)}");
```

