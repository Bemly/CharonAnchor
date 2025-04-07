using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Common;

public class BotKeystore
{
    private BotKeystore()
    {
        
    }
    
    public long Uin { get; set; }
    
    public string Uid { get; set; } = string.Empty;

    internal EcdhProvider Prime256V1 { get; } = new(EllipticCurve.Prime256V1);
    internal EcdhProvider Secp192K1 { get; } = new(EllipticCurve.Secp192K1);
    
    public WLoginSigs WLoginSigs { get; set; } = new();
    
    public byte[] Guid { get; set; } = [];
    public string DeviceName { get; set; } = string.Empty;

    public static BotKeystore CreateEmpty()
    {
        var guid = new byte[16];
        Random.Shared.NextBytes(guid);
        
        return new BotKeystore
        {
            Guid = guid,
            DeviceName = "Lagrange"
        };
    }
}

public class WLoginSigs
{
    public byte[] A2 { get; set; } = [];
    
    public byte[] D2 { get; set; } = [];
    
    public byte[] D2Key { get; set; } = new byte[16];
    
    public byte[] EncryptedA1 { get; set; } = [];
    
    internal byte[]? NoPicSig { get; set; }
    
    internal byte[]? QrSig { get; set; }
    
    public byte[] TgtgtKey { get; set; } = [];
    
    public void Clear()
    {
        A2 = [];
        D2 = [];
        D2Key = new byte[16];
        EncryptedA1 = [];
        NoPicSig = null;
        QrSig = null;
        TgtgtKey = [];
    }
}