using System.Runtime.InteropServices;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Internal.Packets.Login;

internal class WtLogin : StructBase
{
    private static readonly byte[] ServerPublicKey = [0x04, 0x92, 0x8D, 0x88, 0x50, 0x67, 0x30, 0x88, 0xB3, 0x43, 0x26, 0x4E, 0x0C, 0x6B, 0xAC, 0xB8, 0x49, 0x6D, 0x69, 0x77, 0x99, 0xF3, 0x72, 0x11, 0xDE, 0xB2, 0x5B, 0xB7, 0x39, 0x06, 0xCB, 0x08, 0x9F, 0xEA, 0x96, 0x39, 0xB4, 0xE0, 0x26, 0x04, 0x98, 0xB5, 0x1A, 0x99, 0x2D, 0x50, 0x81, 0x3D, 0xA8];

    private readonly byte[] _shareKey;
    
    private readonly BotContext _context;
    
    public WtLogin(BotContext context) : base(context)
    {
        _context = context;
        _shareKey = Keystore.Secp192K1.KeyExchange(ServerPublicKey, true);
    }
    
    public ReadOnlyMemory<byte> BuildTransEmp31()
    {
        using var writer = new BinaryPacket(stackalloc byte[300]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write<ulong>(0); // uin
        writer.Write(ReadOnlySpan<byte>.Empty); // TGT
        writer.Write<byte>(0);
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly);
        
        using var tlvs = new TlvQrCode(_context);
        tlvs.Tlv16();
        tlvs.Tlv18();
        tlvs.Tlv1D();
        tlvs.Tlv33();
        tlvs.Tlv35();
        tlvs.Tlv66();
        tlvs.TlvD1();
        
        writer.Write(tlvs.CreateReadOnlySpan());

        return BuildCode2dPacket(0x31, writer.CreateReadOnlySpan());
    }

    public ReadOnlyMemory<byte> BuildTransEmp12()
    {
        using var writer = new BinaryPacket(stackalloc byte[100]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write(Keystore.WLoginSigs.QrSig, Prefix.Int16 | Prefix.LengthOnly);
        writer.Write<ulong>(0); // uin
        writer.Write(ReadOnlySpan<byte>.Empty); // TGT
        writer.Write<byte>(0);
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly);
        writer.Write<ushort>(0); // tlv count = 0
        
        return BuildCode2dPacket(0x12, writer.CreateReadOnlySpan());
    }

    public ReadOnlyMemory<byte> BuildOicq09()
    {
        using var tlvs = new Tlv(0x09, _context);
        
        tlvs.Tlv106EncryptedA1();
        tlvs.Tlv144();
        tlvs.Tlv116();
        tlvs.Tlv142();
        tlvs.Tlv145();
        tlvs.Tlv018();
        tlvs.Tlv141();
        tlvs.Tlv177();
        tlvs.Tlv191(0);
        tlvs.Tlv100();
        tlvs.Tlv107();
        tlvs.Tlv318();
        tlvs.Tlv16A();
        tlvs.Tlv166();
        tlvs.Tlv521();
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }
    
    public async Task<ReadOnlyMemory<byte>> BuildOicq09Android(string password)
    {
        var sign = (IAndroidBotSignProvider)_context.PacketContext.SignProvider;
        var energy = await sign.GetEnergy(_context.BotUin, "810_9");
        var attach = await sign.GetDebugXwid(_context.BotUin, "810_9");
        
        using var tlvs = new Tlv(0x09, _context);
        
        tlvs.Tlv018Android();
        tlvs.Tlv001();
        tlvs.Tlv106Pwd(password);
        tlvs.Tlv116();
        tlvs.Tlv100Android();
        tlvs.Tlv107Android();
        tlvs.Tlv142();
        tlvs.Tlv144Report();
        tlvs.Tlv145();
        tlvs.Tlv147();
        tlvs.Tlv154();
        tlvs.Tlv141Android();
        tlvs.Tlv008();
        tlvs.Tlv511();
        tlvs.Tlv187();
        tlvs.Tlv188();
        tlvs.Tlv191(0x82);
        tlvs.Tlv177();
        tlvs.Tlv516();
        tlvs.Tlv521Android();
        tlvs.Tlv525();
        tlvs.TlvRaw(0x544, energy);
        tlvs.Tlv545();
        tlvs.TlvRaw(0x548, PowProvider.GenerateTlv548());
        tlvs.TlvRaw(0x553, attach);
        // 542 smsExtraData
        
        return BuildPacket(0x810, tlvs.CreateReadOnlySpan());
    }
    
    private ReadOnlyMemory<byte> BuildPacket(short command, scoped ReadOnlySpan<byte> payload) // corrected
    {
        int cipherLength = TeaProvider.GetCipherLength(payload.Length);
        var writer = new BinaryPacket(cipherLength + 80);
        Span<byte> cipher = stackalloc byte[cipherLength];
        TeaProvider.Encrypt(payload, cipher, _shareKey);
        
        writer.Write((byte)2);
        writer.EnterLengthBarrier<short>();
        
        writer.Write((short)8001); // version
        writer.Write(command);
        writer.Write((short)0); // sequence
        writer.Write((uint)Keystore.Uin);
        writer.Write((byte)3);
        writer.Write((byte)EncryptMethod.EM_ECDH_ST);
        writer.Write(0);
        writer.Write((byte)2);
        writer.Write((short)0); // insId
        writer.Write(AppInfo.AppClientVersion); // insId
        writer.Write(0); // retryTime
        BuildEncryptHead(ref writer);
        writer.Write(cipher);
        writer.Write((byte)3);
        
        writer.ExitLengthBarrier<short>(true, 1);

        return writer.ToArray();
    }

    private ReadOnlyMemory<byte> BuildCode2dPacket(short command, scoped ReadOnlySpan<byte> tlv)
    {
        using var reqBody = new BinaryPacket(stackalloc byte[48 + tlv.Length]);
        reqBody.Write((uint)DateTimeOffset.Now.ToUnixTimeSeconds());
        
        reqBody.Write((byte)2); // encryptMethod == EncryptMethod.EM_ST || encryptMethod == EncryptMethod.EM_ECDH_ST | Section of length 43 + tlv.Length + 1
        reqBody.EnterLengthBarrier<short>();
        reqBody.Write(command);
        reqBody.Skip(21);
        reqBody.Write((byte)3);
        reqBody.Write((short)0x00); // close
        reqBody.Write((short)0x32); // Version Code: 50
        reqBody.Write(0); // trans_emp sequence
        reqBody.Write(Keystore.Uin); // dummy uin
        reqBody.Write(tlv);
        reqBody.Write((byte)3);
        reqBody.ExitLengthBarrier<short>(true, 1);
        
        var reqSpan = reqBody.CreateReadOnlySpan();
        using var writer = new BinaryPacket(stackalloc byte[14 + reqSpan.Length]);
        writer.Write((byte)0x00);
        writer.Write((ushort)reqSpan.Length);
        writer.Write(AppInfo.AppId);
        writer.Write((uint)0x72); // Role
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly); // uSt
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int8 | Prefix.LengthOnly); // rollback
        writer.Write(reqSpan);

        return BuildPacket(0x812, writer.CreateReadOnlySpan());
    }

    private void BuildEncryptHead(ref BinaryPacket writer)
    {
        writer.Write((byte)1);
        writer.Write((byte)1);
        writer.Write(Keystore.WLoginSigs.RandomKey);
        writer.Write((short)0x102); // encrypt type
        writer.Write(Keystore.Secp192K1.PackPublic(true), Prefix.Int16 | Prefix.LengthOnly);
    }

    public ReadOnlySpan<byte> Parse(ReadOnlySpan<byte> input, out ushort command)
    {
        var reader = new BinaryPacket(input);
        
        byte header = reader.Read<byte>();
        ushort length = reader.Read<ushort>();
        ushort version = reader.Read<ushort>();
        command = reader.Read<ushort>();
        ushort sequence = reader.Read<ushort>();
        uint uin = reader.Read<uint>();
        byte flag = reader.Read<byte>();
        byte encryptType = reader.Read<byte>();
        byte state = reader.Read<byte>();
        var encrypted = reader.ReadBytes()[..^1]; // remove last byte

        byte[] key;
        switch (encryptType)
        {
            case 0:
            {
                key = state == 180 ? Keystore.WLoginSigs.RandomKey : _shareKey;
                break;
            }
            case 3:
            {
                key = Keystore.WLoginSigs.WtSessionTicketKey;
                break;
            }
            case 4:
            {
                var raw = TeaProvider.Decrypt(encrypted, _shareKey);
                var rawReader = new BinaryPacket(raw.AsSpan());
                var publicKey = rawReader.ReadBytes(Prefix.Int16 | Prefix.LengthOnly);
                key = Keystore.Secp192K1.KeyExchange(publicKey.ToArray(), true);
                encrypted = rawReader.ReadBytes();
                break;
            }
            default:
            {
                throw new Exception($"Unknown encrypt type: {encryptType}");
            }
        }
        
        var span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(encrypted), encrypted.Length);
        TeaProvider.Decrypt(span, span, key);
        return TeaProvider.CreateDecryptSpan(span);
    }

    public ReadOnlySpan<byte> ParseCode2dPacket(ReadOnlySpan<byte> input, out ushort command)
    {
        var reader = new BinaryPacket(input);
        
        reader.Skip(5);
        byte header = reader.Read<byte>();
        ushort length = reader.Read<ushort>();
        command = reader.Read<ushort>();
        reader.Skip(21);
        byte flag = reader.Read<byte>();
        ushort retryTime = reader.Read<ushort>();
        ushort sequence = reader.Read<ushort>();
        uint uin = reader.Read<uint>();
        ulong timestamp = reader.Read<ulong>();

        return reader.ReadBytes();
    }
    
    public enum EncryptMethod : byte
    {
        EM_ST = 0x45,
        EM_ECDH = 0x07,
        EM_ECDH_ST = 0x87,
        EM_NULL = 0xff
    }
}