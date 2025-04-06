using Lagrange.Core.Common;
using Lagrange.Core.Utility.Binary;

namespace Lagrange.Core.Internal.Packets.Login;

internal ref struct TlvQrCode(BotContext context)
{
    private BinaryPacket _writer = new(); // TODO: Determine the allocation type of the BinaryPacket

    private short _count;
    
    private readonly BotKeystore _keystore = context.Keystore;
    
    private readonly BotAppInfo _appInfo = context.AppInfo;

    public void Tlv16()
    {
        WriteTlv(0x16);
        
        _writer.Write(0u);
        _writer.Write(_appInfo.AppId);
        _writer.Write(_appInfo.SubAppId);
        _writer.Write(_keystore.Guid);
        _writer.Write(_appInfo.PackageName, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(_appInfo.PtVersion, Prefix.Int16 | Prefix.LengthOnly);
        _writer.Write(_appInfo.PackageName, Prefix.Int16 | Prefix.LengthOnly);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv18()
    {
        WriteTlv(0x1B);
        
        _writer.Write(0u); // micro
        _writer.Write(0u); // version
        _writer.Write(3u); // size
        _writer.Write(4u); // margin
        _writer.Write(72u); // dpi
        _writer.Write(2u); // eclevel
        _writer.Write(2u); // hint
        _writer.Write((ushort)0u); // unknown
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv1D()
    {
        WriteTlv(0x1D);
        
        _writer.Write((byte)1u);
        _writer.Write(_appInfo.SdkInfo.MiscBitMap);
        _writer.Write(0u);
        _writer.Write((byte)0);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv33()
    {
        WriteTlv(0x33);
        
        _writer.Write(_keystore.Guid);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv35()
    {
        WriteTlv(0x35);
        
        _writer.Write(_appInfo.SsoVersion);
        
        _writer.ExitLengthBarrier<short>(false);
    }
    
    public void Tlv66()
    {
        WriteTlv(0x66);
        
        _writer.Write(_appInfo.SsoVersion);
        
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