using System.Security.Cryptography;
using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Proto;

namespace Lagrange.Core.Internal.Packets.Login;

internal ref struct Tlv : IDisposable
{
    private BinaryPacket _writer;

    private short _count;
    private bool _prefixed;
    
    private readonly BotKeystore _keystore;
    private readonly BotAppInfo _appInfo;
    private readonly BotContext _context;

    public Tlv(short command, BotContext context)
    {
        _context = context;
        _keystore = context.Keystore;
        _appInfo = context.AppInfo;
        
        _writer = new BinaryPacket(500);
        if (command > 0)
        {
            _writer.Write(command);
            _prefixed = true;
        }
        _writer.Skip(2);
    }

    public void Tlv018()
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
        _writer.Write(_appInfo.SsoVersion); // sso ver, dont over 7
        _writer.Write(_appInfo.AppId);
        _writer.Write(_appInfo.SubAppId);
        _writer.Write(8001u); // app client ver
        _writer.Write(_appInfo.SdkInfo.MainSigMap);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv106Pwd(string password)
    {
        var md5 = MD5.HashData(Encoding.UTF8.GetBytes(password));
        
        var plainWriter = new BinaryPacket(stackalloc byte[100]);
        plainWriter.Write<short>(4); // TGTGT Version
        plainWriter.Write(Random.Shared.Next());
        plainWriter.Write(_appInfo.SsoVersion);
        plainWriter.Write(_appInfo.AppId);
        plainWriter.Write<int>(_appInfo.AppClientVersion);
        plainWriter.Write(_keystore.Uin);
        plainWriter.Write((int)DateTimeOffset.Now.ToUnixTimeSeconds());
        plainWriter.Write(0); // dummy IP Address
        plainWriter.Write<byte>(1);
        plainWriter.Write(md5);
        plainWriter.Write(_keystore.WLoginSigs.TgtgtKey);
        plainWriter.Write(0);  // unknown
        plainWriter.Write<byte>(1); // guidAvailable
        plainWriter.Write(_keystore.Guid);
        plainWriter.Write(_appInfo.SubAppId);
        plainWriter.Write(1); // flag
        plainWriter.Write(_keystore.Uin.ToString(), Prefix.Int16 | Prefix.LengthOnly);
        plainWriter.Write<short>(0);
        
        var keyWriter = new BinaryPacket(stackalloc byte[16 + 4 + 4]);
        keyWriter.Write(md5);
        keyWriter.Write(0); // empty 4 bytes
        keyWriter.Write((uint)_keystore.Uin);
        var key = MD5.HashData(keyWriter.CreateReadOnlySpan());

        WriteTlv(0x106);
        
        var encrypted = TeaProvider.Encrypt(plainWriter.CreateReadOnlySpan(), key);
        _writer.Write(encrypted);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public void Tlv106EncryptedA1()
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
        var tlv = new Tlv(-1, _context);
        
        tlv.Tlv16E();
        tlv.Tlv147();
        tlv.Tlv128();
        tlv.Tlv124();

        var span = tlv._writer.CreateReadOnlySpan();
        Span<byte> encrypted = stackalloc byte[TeaProvider.GetCipherLength(span.Length)];
        TeaProvider.Encrypt(span, encrypted, _keystore.WLoginSigs.TgtgtKey);
        tlv.Dispose();
        
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
    
    public void Tlv52D()
    {
        WriteTlv(0x52D);
        
        var report = new DeviceReport
        {
            AndroidId = "unknown",
            Baseband = "Linux version 4.19.157-perf-g92c089fc2d37 (builder@pangu-build-component-vendor-272092-qncbv-vttl3-61r9m) (clang version 10.0.7 for Android NDK, GNU ld (binutils-2.27-bd24d23f) 2.27.0.20170315) #1 SMP PREEMPT Wed Jun 5 13:27:08 UTC 2024",
            BootId = "REL",
            Bootloader = "V816.0.6.0.TKHCNXM",
            Codename = "Redmi/alioth/alioth:13/TKQ1.221114.001/V816.0.6.0.TKHCNXM:user/release-keys",
            Incremental = "3ed8347e5a15e7c3",
            InnerVer = "",
            Version = "V816.0.6.0.TKHCNXM"
        };
        ProtoHelper.Serialize(ref _writer, report);
        
        _writer.ExitLengthBarrier<short>(false);
    }

    public ReadOnlySpan<byte> CreateReadOnlySpan()
    {
        _writer.Write(_count, _prefixed ? 2 : 0);
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

[ProtoPackable]
internal partial class DeviceReport
{
    [ProtoMember(1)] public string? AndroidId { get; set; }
    
    [ProtoMember(2)] public string? Baseband { get; set; }
    
    [ProtoMember(3)] public string? BootId { get; set; }
    
    [ProtoMember(4)] public string? Bootloader { get; set; }
    
    [ProtoMember(5)] public string? Codename { get; set; }
    
    [ProtoMember(6)] public string? Fingerprint { get; set; }
    
    [ProtoMember(7)] public string? Incremental { get; set; }
    
    [ProtoMember(8)] public string? InnerVer { get; set; }
    
    [ProtoMember(9)] public string? Version { get; set; }
}