using System.Runtime.InteropServices;
using Lagrange.Core.Common;

namespace CharonAnchor;

/// <summary>
/// Charon sign provider that directly calls wrapper.node
/// Inherits from BotSignProvider to integrate with Lagrange.Milky
/// </summary>
public class CharonSignProvider : BotSignProvider, IDisposable
{
    private readonly WrapperLoader _loader;
    private readonly WrapperInterop.SignFuncV1Delegate? _signFuncV1;
    private readonly WrapperInterop.SignFuncV2Delegate? _signFuncV2;
    private readonly bool _useV2;
    private bool _disposed = false;

    // PC whitelist commands (copied from Lagrange.Milky/Utility/Signer.cs)
    private static readonly HashSet<string> PcWhiteListCommand = new()
    {
        "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey",
        "trpc.o3.ecdh_access.EcdhAccess.SsoSecureAccess",
        "trpc.o3.report.Report.SsoReport",
        "MessageSvc.PbSendMsg",
        "wtlogin.trans_emp",
        "wtlogin.login",
        "wtlogin.exchange_emp",
        "trpc.login.ecdh.EcdhService.SsoKeyExchange",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginNewDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshTicket",
        "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshA2",
        "OidbSvcTrpcTcp.0x11ec_1",
        "OidbSvcTrpcTcp.0x758_1",
        "OidbSvcTrpcTcp.0x7c1_1",
        "OidbSvcTrpcTcp.0x7c2_5",
        "OidbSvcTrpcTcp.0x10db_1",
        "OidbSvcTrpcTcp.0x8a1_7",
        "OidbSvcTrpcTcp.0x89a_0",
        "OidbSvcTrpcTcp.0x89a_15",
        "OidbSvcTrpcTcp.0x88d_0",
        "OidbSvcTrpcTcp.0x88d_14",
        "OidbSvcTrpcTcp.0x112a_1",
        "OidbSvcTrpcTcp.0x587_74",
        "OidbSvcTrpcTcp.0x1100_1",
        "OidbSvcTrpcTcp.0x1102_1",
        "OidbSvcTrpcTcp.0x1103_1",
        "OidbSvcTrpcTcp.0x1107_1",
        "OidbSvcTrpcTcp.0x1105_1",
        "OidbSvcTrpcTcp.0xf88_1",
        "OidbSvcTrpcTcp.0xf89_1",
        "OidbSvcTrpcTcp.0xf57_1",
        "OidbSvcTrpcTcp.0xf57_106",
        "OidbSvcTrpcTcp.0xf57_9",
        "OidbSvcTrpcTcp.0xf55_1",
        "OidbSvcTrpcTcp.0xf67_1",
        "OidbSvcTrpcTcp.0xf67_5",
        "OidbSvcTrpcTcp.0x6d9_4",
    };

    public CharonSignProvider(string? workingDirectory = null, string version = "3.2.28")
    {
        _loader = new WrapperLoader(workingDirectory, version);

        // V2 (int return) for 3.2.28+, V1 (long return) for 3.2.19
        _useV2 = version != "3.2.19";

        if (!_loader.Initialize())
        {
            throw new InvalidOperationException("Failed to initialize wrapper loader");
        }

        if (_useV2)
        {
            _signFuncV2 = _loader.GetSignFunc<WrapperInterop.SignFuncV2Delegate>();
            Console.WriteLine("Using V2 signature function");
        }
        else
        {
            _signFuncV1 = _loader.GetSignFunc<WrapperInterop.SignFuncV1Delegate>();
            Console.WriteLine("Using V1 signature function");
        }
    }

    public override bool IsWhiteListCommand(string cmd) => PcWhiteListCommand.Contains(cmd);

    public override unsafe Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        return Task.Run(() =>
        {
            // Allocate output buffer
            var outputBuffer = Marshal.AllocHGlobal(WrapperInterop.OutputBufferSize);
            try
            {
                // Pin body data
                var bodyHandle = body.Pin();
                try
                {
                    int result;
                    if (_useV2 && _signFuncV2 != null)
                    {
                        result = _signFuncV2(cmd, (IntPtr)bodyHandle.Pointer, body.Length, seq, outputBuffer);
                    }
                    else if (_signFuncV1 != null)
                    {
                        result = (int)_signFuncV1(cmd, (IntPtr)bodyHandle.Pointer, body.Length, seq, outputBuffer);
                    }
                    else
                    {
                        return null;
                    }

                    if (result != 0)
                    {
                        Console.WriteLine($"Sign call failed with code: {result}");
                        return null;
                    }

                    return ExtractSecInfo(outputBuffer);
                }
                finally
                {
                    bodyHandle.Dispose();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(outputBuffer);
            }
        });
    }

    private unsafe SsoSecureInfo ExtractSecInfo(IntPtr buffer)
    {
        var tokenLen = Marshal.ReadByte(buffer + WrapperInterop.TokenLenOffset);
        var extraLen = Marshal.ReadByte(buffer + WrapperInterop.ExtraLenOffset);
        var signLen = Marshal.ReadByte(buffer + WrapperInterop.SignLenOffset);

        var token = new byte[tokenLen];
        var extra = new byte[extraLen];
        var sign = new byte[signLen];

        Marshal.Copy(buffer + WrapperInterop.TokenDataOffset, token, 0, tokenLen);
        Marshal.Copy(buffer + WrapperInterop.ExtraDataOffset, extra, 0, extraLen);
        Marshal.Copy(buffer + WrapperInterop.SignDataOffset, sign, 0, signLen);

        return new SsoSecureInfo
        {
            SecToken = token,
            SecExtra = extra,
            SecSign = sign,
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _loader.Dispose();
            _disposed = true;
        }
    }
}