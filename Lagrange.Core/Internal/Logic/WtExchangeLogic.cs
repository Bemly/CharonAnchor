using Lagrange.Core.Common;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Services.System;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using ThirdPartyLoginResponse = Lagrange.Core.Internal.Packets.System.ThirdPartyLoginResponse;

namespace Lagrange.Core.Internal.Logic;

internal class WtExchangeLogic : ILogic, IDisposable
{
    private const string Tag = nameof(WtExchangeLogic);
    
    private readonly BotContext _context;
    
    private readonly Timer _heartBeatTimer;

    private readonly Timer _ssoHeartBeatTimer;

    private readonly Timer _queryStateTimer;

    private CancellationToken? _token;

    private TaskCompletionSource<bool>? _transEmpSource;

    private TaskCompletionSource<(string, string)>? _captchaSource;

    private TaskCompletionSource<string>? _smsSource;

    public WtExchangeLogic(BotContext context)
    {
        _context = context;
        _heartBeatTimer = new Timer(OnHeartBeat);
        _ssoHeartBeatTimer = new Timer(OnSsoHeartBeat);
        _queryStateTimer = new Timer(OnQueryState);
    }

    public async Task<bool> Login(long uin, string? password, CancellationToken token)
    {
        _token = token;

        token.UnsafeRegister(_ =>
        {
            _transEmpSource?.TrySetCanceled();
            _captchaSource?.TrySetCanceled();
            _smsSource?.TrySetCanceled();
        }, null);
        
        if (!_context.SocketContext.Connected)
        {
            await _context.SocketContext.Connect();
            _heartBeatTimer.Change(0, 2000);
        }

        if (_context.Keystore.WLoginSigs is { D2.Length: not 0, A2.Length: not 0 })
        {
            bool online = await Online();
            if (online) return true;
        }

        return await ManualLogin(uin, password);
    }

    private async Task<bool> ManualLogin(long uin, string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            var transEmp31 = await _context.EventContext.SendEvent<TransEmp31EventResp>(new TransEmp31EventReq());
            if (transEmp31 == null) return false; // TODO: Log error

            _transEmpSource = new TaskCompletionSource<bool>();
            _context.EventInvoker.PostEvent(new BotQrCodeEvent(transEmp31.Url, transEmp31.Image));
            
            _context.Keystore.WLoginSigs.QrSig = transEmp31.QrSig;
            _queryStateTimer.Change(0, 2000);
            bool isLoginSuccess = await _transEmpSource.Task;

            if (isLoginSuccess) return await Online();
        }
        else
        {
            
        }
        
        return false;
    }

    private async Task<bool> Online()
    {
        var infoSync = await _context.EventContext.SendEvent<InfoSyncEventResp>(new InfoSyncEventReq());
        if (infoSync == null) return false;

        if (infoSync.Message == "register success")
        {
            _context.EventInvoker.PostEvent(new BotOnlineEvent(BotOnlineEvent.Reasons.Login));
            _context.IsOnline = true;
            
            _ssoHeartBeatTimer.Change(0, 270 * 1000);
            return true;
        }

        return false;
    }

    private void OnHeartBeat(object? state) => Task.Run(async () =>
    {
        await _context.EventContext.SendEvent<AliveEvent>(new AliveEvent());
    });
    
    private void OnSsoHeartBeat(object? state) => Task.Run(async () =>
    {
        await _context.EventContext.SendEvent<SsoHeartBeatEventResp>(new SsoHeartBeatEventReq());
    });
    
    private void OnQueryState(object? state) => Task.Run(async () =>
    {
        if (_transEmpSource == null) return;
        var transEmp12 = await _context.EventContext.SendEvent<TransEmp12EventResp>(new TransEmp12EventReq());
        if (transEmp12 == null) return;
        
        _context.EventInvoker.PostEvent(new BotQrCodeQueryEvent((BotQrCodeQueryEvent.TransEmpState)transEmp12.State));
        
        switch (transEmp12)
        {
            case { State: TransEmp12EventResp.TransEmpState.Confirmed, Data: { } data }:
                _context.Keystore.WLoginSigs.TgtgtKey = data.TgtgtKey;
                _context.Keystore.WLoginSigs.NoPicSig = data.NoPicSig;
                _context.Keystore.WLoginSigs.A1 = data.TempPassword;
                _context.Keystore.Uin = transEmp12.Uin;

                _queryStateTimer.Change(Timeout.Infinite, Timeout.Infinite);

                var result = await _context.EventContext.SendEvent<LoginEventResp>(new LoginEventReq(LoginEventReq.Command.Tgtgt));

                if (result?.RetCode == 0)
                {
                    ReadWLoginSigs(result.Tlvs);
                    _transEmpSource.TrySetResult(true);
                }
                else
                {
                    _context.LogError(Tag, $"Login failed: {result?.RetCode} | Message: {result?.Error}");
                    _transEmpSource.TrySetResult(false);
                }
                
                _context.EventInvoker.PostEvent(new BotLoginEvent(result?.RetCode ?? byte.MaxValue, result?.Error));
                break;
            case { State: TransEmp12EventResp.TransEmpState.Canceled or TransEmp12EventResp.TransEmpState.Invalid or TransEmp12EventResp.TransEmpState.CodeExpired }:
                _context.LogCritical(Tag, $"QR Code State: {transEmp12.State}");
                
                _transEmpSource.TrySetResult(false);
                _queryStateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                break;
        }
    });

    private void ReadWLoginSigs(Dictionary<ushort, byte[]> tlvs)
    {
        foreach (var (tag, value) in tlvs)
        {
            switch (tag)
            {
                case 0x103:
                    _context.Keystore.WLoginSigs.StWeb = value;
                    break;
                case 0x143:
                    _context.Keystore.WLoginSigs.D2 = value;
                    break;
                case 0x108:
                    _context.Keystore.WLoginSigs.Ksid = value;
                    break;
                case 0x10A:
                    _context.Keystore.WLoginSigs.A2 = value;
                    break;
                case 0x10C:
                    _context.Keystore.WLoginSigs.A1Key = value;
                    break;
                case 0x10D:
                    _context.Keystore.WLoginSigs.A2Key = value;
                    break;
                case 0x10E:
                    _context.Keystore.WLoginSigs.StKey = value;
                    break;
                case 0x114:
                    _context.Keystore.WLoginSigs.St = value;
                    break;
                case 0x11A:
                    var reader = new BinaryPacket(value.AsSpan());
                    reader.Read<ushort>(); // FaceId
                    byte age = reader.Read<byte>();
                    byte gender = reader.Read<byte>();
                    string nickname = reader.ReadString(Prefix.Int8 | Prefix.LengthOnly);
                    _context.BotInfo = new BotInfo(age, gender, nickname);
                    break;
                case 0x133:
                    _context.Keystore.WLoginSigs.WtSessionTicket = value;
                    break;
                case 0x134:
                    _context.Keystore.WLoginSigs.WtSessionTicketKey = value;
                    break;
                case 0x305:
                    _context.Keystore.WLoginSigs.D2Key = value;
                    break;
                case 0x106:
                    _context.Keystore.WLoginSigs.A1 = value;
                    break;
                case 0x16A:
                    _context.Keystore.WLoginSigs.NoPicSig = value;
                    break;
                case 0x16D:
                    _context.Keystore.WLoginSigs.SuperKey = value;
                    break;
                case 0x543:
                    var resp = ProtoHelper.Deserialize<ThirdPartyLoginResponse>(value);
                    _context.Keystore.Uid = resp.CommonInfo.RspNT.Uid;
                    break;
                default:
                    _context.LogTrace(Tag, $"Unknown TLV: {tag:X}");
                    break;
            }
        }
    }

    public void Dispose()
    {
        _transEmpSource?.TrySetCanceled();
        _captchaSource?.TrySetCanceled();
        _smsSource?.TrySetCanceled();
        
        _heartBeatTimer.Dispose();
        _ssoHeartBeatTimer.Dispose();
        _queryStateTimer.Dispose();
    }
}