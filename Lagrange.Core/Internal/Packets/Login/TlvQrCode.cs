using Lagrange.Core.Common;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using ProtoBuf;

namespace Lagrange.Core.Internal.Packets.Login;

internal ref struct TlvQrCode : IDisposable
{
    private BinaryPacket _writer;

    private short _count;
    
    private readonly BotKeystore _keystore;
    
    private readonly BotAppInfo _appInfo;

    public TlvQrCode(BotContext context)
    {
        _keystore = context.Keystore;
        _appInfo = context.AppInfo;
        
        _writer = new BinaryPacket(300);
        _writer.Skip(2);
    }

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

    public void TlvD1()
    {
        WriteTlv(0xD1);
        
        var obj = new TlvQrCodeD1
        {
            Sys = new NTQrCodeInfo
            {
                OS = _appInfo.Os,
                Name = _keystore.DeviceName
            },
            Type = [0x30, 0xD1]
        };
        ProtoHelper.Serialize(ref _writer, obj);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public ReadOnlySpan<byte> CreateReadOnlySpan()
    {
        _writer.Write(_count, 0);
        return _writer.CreateReadOnlySpan();
    }
    
    private void WriteTlv(short tag)
    {
        _writer.Write(tag);
        _writer.EnterLengthBarrier<short>();
        _count++;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}

#pragma warning disable CS8618

[ProtoContract]
internal class TlvQrCodeD1
{
    [ProtoMember(1)] public NTQrCodeInfo Sys { get; set; }
    
    [ProtoMember(2)] public string Url { get; set; }
    
    [ProtoMember(3)] public string QrSig { get; set; }
    
    [ProtoMember(4)] public byte[] Type { get; set; }
}

[ProtoContract]
internal class NTQrCodeInfo
{
    [ProtoMember(1)] public string OS { get; set; }
    
    [ProtoMember(2)] public string Name { get; set; }
}