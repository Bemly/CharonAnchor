using System.Text.Json.Serialization;

namespace Lagrange.Core.Common;

[Serializable]
public class BotInfo
{
    [JsonConstructor]
    internal BotInfo(byte age, byte gender, string name)
    {
        Age = age;
        Gender = gender;
        Name = name;
    }
    
    public byte Age { get; set; }
        
    public byte Gender { get; set; }
        
    public string Name { get; set; }

    public override string ToString() => $"Bot name: {Name} | Gender: {Gender} | Age: {Age}";
}