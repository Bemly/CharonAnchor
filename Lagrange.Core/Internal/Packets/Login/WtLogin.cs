using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Internal.Packets.Login;

internal class WtLogin(BotContext context) : StructBase(context)
{
    private static readonly byte[] ServerPublicKey = [0x04, 0x92, 0x8D, 0x88, 0x50, 0x67, 0x30, 0x88, 0xB3, 0x43, 0x26, 0x4E, 0x0C, 0x6B, 0xAC, 0xB8, 0x49, 0x6D, 0x69, 0x77, 0x99, 0xF3, 0x72, 0x11, 0xDE, 0xB2, 0x5B, 0xB7, 0x39, 0x06, 0xCB, 0x08, 0x9F, 0xEA, 0x96, 0x39, 0xB4, 0xE0, 0x26, 0x04, 0x98, 0xB5, 0x1A, 0x99, 0x2D, 0x50, 0x81, 0x3D, 0xA8];
    
    public ReadOnlyMemory<byte> BuildTransEmp31()
    {
        using var writer = new BinaryPacket(stackalloc byte[300]);
        writer.Write<ushort>(0);
        writer.Write(AppInfo.AppId);
        writer.Write<ulong>(0); // uin
        writer.Write(ReadOnlySpan<byte>.Empty); // TGT
        writer.Write<byte>(0);
        writer.Write(ReadOnlySpan<byte>.Empty, Prefix.Int16 | Prefix.LengthOnly);
        
        var tlvs = new TlvQrCode(context);
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
    
    private ReadOnlyMemory<byte> BuildPacket(short command, scoped ReadOnlySpan<byte> payload) // corrected
    {
        Console.WriteLine(Convert.ToHexString(payload));
        
        var sharedKey = Keystore.Secp192K1.KeyExchange(ServerPublicKey, true);
        int cipherLength = TeaProvider.GetCipherLength(payload.Length);
        var writer = new BinaryPacket(cipherLength + 80);
        
        writer.Write((byte)2);
        writer.EnterLengthBarrier<short>();
        
        writer.Write((short)8001);
        writer.Write(command);
        writer.Write((short)0); // sequence
        writer.Write((uint)Keystore.Uin);
        writer.Write((byte)3);
        writer.Write((byte)135);
        writer.Write(0);
        writer.Write((byte)19);
        writer.Write((short)0); // insId
        writer.Write(AppInfo.AppClientVersion); // insId
        writer.Write(0); // retryTime
        BuildEncryptHead(ref writer);
        TeaProvider.Encrypt(payload, writer.CreateSpan(cipherLength), sharedKey);
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
        Span<byte> random = stackalloc byte[16];
        Random.Shared.NextBytes(random);
        
        writer.Write((byte)1);
        writer.Write((byte)1);
        writer.Write(random);
        writer.Write((short)0x102); // encrypt type
        writer.Write(Keystore.Secp192K1.PackPublic(true), Prefix.Int16 | Prefix.LengthOnly);
    }
}