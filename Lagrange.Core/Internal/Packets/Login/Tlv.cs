using Lagrange.Core.Common;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Internal.Packets.Login;

internal ref struct Tlv(BotContext context)
{
    private BinaryPacket _writer = new(); // TODO: Determine the allocation type of the BinaryPacket

    private short _count;
    
    private readonly BotKeystore _keystore = context.Keystore;
    
    private readonly BotAppInfo _appInfo = context.AppInfo;

    public void Tlv18()
    {
        WriteTlv(0x18);
        
        _writer.Write((short)0);
        _writer.Write(5u);
        _writer.Write(0u);
        _writer.Write(8001u); // app client ver
        _writer.Write((uint)_keystore.Uin);
        _writer.Write((short)0);
        _writer.Write((short)0);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv100()
    {
        WriteTlv(0x100);
        
        _writer.Write((ushort)0); // db buf ver
        _writer.Write(5u); // sso ver, dont over 7
        _writer.Write(_appInfo.AppId);
        _writer.Write(_appInfo.SubAppId);
        _writer.Write(8001u); // app client ver
        _writer.Write(_appInfo.SdkInfo.MainSigMap);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv106A2()
    {
        WriteTlv(0x106);
        
        _writer.Write(_keystore.WLoginSigs.EncryptedA1);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv107()
    {
        WriteTlv(0x107);

        _writer.Write((ushort)1); // pic type
        _writer.Write((byte)0x0D); // captcha type
        _writer.Write((ushort)0); // pic size
        _writer.Write((byte)1); // ret type
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv116()
    {
        WriteTlv(0x116);
        
        _writer.Write((byte)0); // version
        _writer.Write(_appInfo.SdkInfo.MiscBitMap); // miscBitMap
        _writer.Write(_appInfo.SdkInfo.SubSigMap);
        _writer.Write((byte)0); // length of subAppId
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv124()
    {
        WriteTlv(0x124);
        
        _writer.Skip(12);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv128()
    {
        WriteTlv(0x128);
        
        _writer.Write((ushort)0);
        _writer.Write((byte)0); // guid new
        _writer.Write((byte)0); // guid available
        _writer.Write((byte)0); // guid changed
        _writer.Write(0u); // guid flag
        _writer.Write(_appInfo.Os, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(_keystore.Guid, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(ReadOnlySpan<char>.Empty, Prefix.Int16 | Prefix.LengthOnly); // brand
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv141()
    {
        WriteTlv(0x141);
        
        _writer.Write((ushort)0);
        _writer.Write("Unknown", Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(0u);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv142()
    {
        WriteTlv(0x142);
        
        _writer.Write((ushort)0);
        _writer.Write(_appInfo.PackageName, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv144()
    {
        var tlv = new Tlv(context);
        
        tlv.Tlv16E();
        tlv.Tlv147();
        tlv.Tlv128();
        tlv.Tlv124();

        var span = tlv._writer.CreateReadOnlySpan();
        Span<byte> encrypted = stackalloc byte[TeaProvider.GetCipherLength(span.Length)];
        TeaProvider.Encrypt(span, encrypted, _keystore.WLoginSigs.TgtgtKey);
        
        WriteTlv(0x144);
        
        _writer.Write(encrypted);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv145()
    {
        WriteTlv(0x145);

        _writer.Write(_keystore.Guid);

        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv147()
    {
        WriteTlv(0x147);
        
        _writer.Write(_appInfo.AppId);
        _writer.Write(_appInfo.PtVersion, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(_appInfo.ApkSignatureMd5, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv166()
    {
        WriteTlv(0x166);
        
        _writer.Write((byte)5);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv16A()
    {
        WriteTlv(0x16A);

        _writer.Write(_keystore.WLoginSigs.NoPicSig);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv16E()
    {
        WriteTlv(0x16E);
        
        _writer.Write(_keystore.DeviceName);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv177()
    {
        WriteTlv(0x177);
        
        _writer.Write((byte)1);
        _writer.Write(0); // sdk build time
        _writer.Write(_appInfo.SdkInfo.SdkVersion, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv191()
    {
        WriteTlv(0x191);
        
        _writer.Write((byte)0);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv318()
    {
        WriteTlv(0x318);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv521()
    {
        WriteTlv(0x521);
        
        _writer.Write(0x13);
        _writer.Write("basicim", Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void WriteTo(ref BinaryPacket writer)
    {
        writer.Write(_count);
        writer.Write(_writer.CreateReadOnlySpan());
        
        _writer.Dispose();
    }

    private void WriteTlv(short tag)
    {
        _writer.Write(tag);
        _writer.EnterLengthBarrier<short>();
        _count++;
    }
}