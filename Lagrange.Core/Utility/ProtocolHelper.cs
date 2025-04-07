using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Utility;

internal static class ProtocolHelper
{
    public static Dictionary<ushort, byte[]> TlvUnPack(ref BinaryPacket reader)
    {
        ushort count = reader.Read<ushort>();
        var tlv = new Dictionary<ushort, byte[]>(count);

        for (int i = 0; i < count; i++)
        {
            ushort tag = reader.Read<ushort>();
            ushort length = reader.Read<ushort>();
            
            var data = new byte[length];
            reader.ReadBytes(data.AsSpan());
            tlv[tag] = data;
        }
        
        return tlv;
    }
}