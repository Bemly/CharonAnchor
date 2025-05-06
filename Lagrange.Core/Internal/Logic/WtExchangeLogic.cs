using System.Text;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Response;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Services.Login;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Cryptography;
using Lagrange.Core.Utility.Extension;
using ThirdPartyLoginResponse = Lagrange.Core.Internal.Packets.System.ThirdPartyLoginResponse;

namespace Lagrange.Core.Internal.Logic;

internal class WtExchangeLogic : ILogic, IDisposable
{
    private const string Tag = nameof(WtExchangeLogic);
    
    private readonly BotContext _context;
    
    private readonly Timer _heartBeatTimer;

    private readonly Timer _ssoHeartBeatTimer;

    private readonly Timer _queryStateTimer;

    private readonly Timer _exchangeEmpTimer;

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
        _exchangeEmpTimer = new Timer(OnExchangeEmp);
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
            _context.LogInfo(Tag, "Valid session detected, doing online task");

            bool online = await Online();
            if (online)
            {
                if (_context.Config.Protocol.IsAndroid()) _exchangeEmpTimer.Change(TimeSpan.Zero, TimeSpan.FromDays(1));
                return true;
            }
        }

        return await ManualLogin(uin, password);
    }
    
    private async Task<bool> ManualLogin(long uin, string? password)
    {
        if (string.IsNullOrEmpty(password) && _context.Config.Protocol.IsAndroid())
        {
            _context.LogError(Tag, "Android Platform can not use QRLogin, Please fill in password");
            return false;
        }

        if (_context.Config.Protocol.IsPC() && _context.Keystore.WLoginSigs is { A1.Length: not 0 })
        {
            if (!await KeyExchange()) return false;
            
            // TODO: EasyLogin
        }
        
        if (string.IsNullOrEmpty(password))
        {
            _context.LogInfo(Tag, "Password is empty or null, use QRCode Login");
            
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
            _context.LogInfo(Tag, "Password is filled, try to login");

            if (_context.Config.Protocol.IsAndroid())
            {
                _context.Keystore.Uin = uin;
                _context.Keystore.WLoginSigs.TgtgtKey = new byte[16];
                Random.Shared.NextBytes(_context.Keystore.WLoginSigs.TgtgtKey);
                
                var result = await _context.EventContext.SendEvent<LoginEventResp>(new LoginEventReq(LoginEventReq.Command.Tgtgt, password));
                if (result == null) return false;

                if (result.State == LoginEventResp.States.CaptchaVerify)
                {
                    if (result.Tlvs.TryGetValue(0x104, out var tlv104))
                    {
                        _context.Keystore.State.Tlv104 = tlv104;
                        _context.LogDebug(Tag, $"Tlv104 received, length: {tlv104.Length}");
                    }
                    
                    if (result.Tlvs.TryGetValue(0x546, out var tlv546))
                    {
                        _context.Keystore.State.Tlv547 = PowProvider.GenerateTlv547(tlv546);
                        _context.LogDebug(Tag, $"Tlv546 received, calculated Tlv547 with length {_context.Keystore.State.Tlv547.Length}");
                    }
                    
                    string captchaUrl = Encoding.UTF8.GetString(result.Tlvs[0x192]);
                    _context.LogInfo(Tag, $"Captcha required, URL: {captchaUrl}");
                    _context.EventInvoker.PostEvent(new BotCaptchaEvent(captchaUrl));
                    
                    _captchaSource = new TaskCompletionSource<(string, string)>();
                    var (ticket, _) = await _captchaSource.Task;
                    _context.LogInfo(Tag, $"Captcha ticket: {ticket}, try to login");
                    
                    _token?.ThrowIfCancellationRequested();
                    result = await _context.EventContext.SendEvent<LoginEventResp>(new LoginEventReq(LoginEventReq.Command.Captcha) { Ticket = ticket });
                    if (result == null) return false;
                }

                if (result.State == LoginEventResp.States.DeviceLockViaSmsNewArea)
                {
                    if (result.Tlvs.TryGetValue(0x104, out var tlv104))
                    {
                        _context.Keystore.State.Tlv104 = tlv104;
                        _context.LogDebug(Tag, $"Tlv104 received, length: {tlv104.Length}");
                    }
                    
                    if (result.Tlvs.TryGetValue(0x174, out var tlv174))
                    {
                        _context.Keystore.State.Tlv174 = tlv174;
                        _context.LogDebug(Tag, $"Tlv174 received, length: {tlv174.Length}");
                    }
                    
                    string? url = null;
                    if (result.Tlvs.TryGetValue(0x204, out var tlv204)) url = Encoding.UTF8.GetString(tlv204);

                    var tlv178 = new BinaryPacket(result.Tlvs[0x178].AsSpan());
                    string countryCode = tlv178.ReadString(Prefix.Int16 | Prefix.LengthOnly);
                    string phone = tlv178.ReadString(Prefix.Int16 | Prefix.LengthOnly);
                    
                    _token?.ThrowIfCancellationRequested();
                    result = await _context.EventContext.SendEvent<LoginEventResp>(new LoginEventReq(LoginEventReq.Command.FetchSMSCode));
                    if (result?.State == LoginEventResp.States.SmsRequired)
                    {
                        if (result.Tlvs.TryGetValue(0x104, out var tlv1048))
                        {
                            _context.Keystore.State.Tlv104 = tlv1048;
                            _context.LogDebug(Tag, $"Tlv104 received, length: {tlv1048.Length}");
                        }
                        
                        _context.LogInfo(Tag, $"SMS Verification required, Phone: {countryCode}-{phone} | URL: {url}");
                        _context.EventInvoker.PostEvent(new BotSMSEvent(url, $"{countryCode}-{phone}"));
                        
                        _smsSource = new TaskCompletionSource<string>();
                        string code = await _smsSource.Task;
                        result = await _context.EventContext.SendEvent<LoginEventResp>(new LoginEventReq(LoginEventReq.Command.SubmitSMSCode) { Code = code });
                    }
                }
                
                if (result?.State == LoginEventResp.States.Success)
                {
                    ReadWLoginSigs(result.Tlvs);
                    
                    _exchangeEmpTimer.Change(TimeSpan.Zero, TimeSpan.FromDays(1));
                    _context.EventInvoker.PostEvent(new BotLoginEvent(0, null));
                    _context.EventInvoker.PostEvent(new BotRefreshKeystoreEvent(_context.Keystore));

                    return await Online();
                }
                else
                {
                    _context.LogError(Tag, $"Login failed: {result?.RetCode} | Message: {result?.Error}");
                    _context.EventInvoker.PostEvent(new BotLoginEvent(result?.RetCode ?? byte.MaxValue, result?.Error));
                }
            }
            else
            {
                if (_context.Keystore.State.KeyExchangeSession is null && !await KeyExchange()) return false;

                var result = await _context.EventContext.SendEvent<PasswordLoginEventResp>(new PasswordLoginEventReq(password, null));
                while (true)
                {
                    if (result == null)
                    {
                        _context.LogError(Tag, "Unexpected null result for trpc.login.*");
                        return false;
                    }
                    _token?.ThrowIfCancellationRequested();
                    
                    switch (result.State)
                    {
                        case NTLoginCommon.State.LOGIN_ERROR_SUCCESS:
                            _context.EventInvoker.PostEvent(new BotLoginEvent(0, null));
                            _context.EventInvoker.PostEvent(new BotRefreshKeystoreEvent(_context.Keystore));
                            return await Online();
                        case NTLoginCommon.State.LOGIN_ERROR_PROOFWATER:
                            _context.LogInfo(Tag, $"Captcha required, URL: {result.JumpingUrl}");
                            _context.EventInvoker.PostEvent(new BotCaptchaEvent(result.JumpingUrl));
                            _captchaSource = new TaskCompletionSource<(string, string)>();

                            string sid = result.JumpingUrl.Split("&sid=")[1].Split("&")[0];
                            var (ticket, randStr) = await _captchaSource.Task;
                            result = await _context.EventContext.SendEvent<PasswordLoginEventResp>(new PasswordLoginEventReq(password, (ticket, randStr, sid)));
                            break;
                        default:
                            _context.LogError(Tag, $"Login failed: {result.State} | Message: {result.Tips}");
                            _context.EventInvoker.PostEvent(new BotLoginEvent((int)result.State, result.Tips));
                            break;
                    }
                }
            }
        }
        
        return false;
    }
    
    public async Task<long> ResolveUinByQid(string qid)
    {
        if (!_context.SocketContext.Connected)
        {
            await _context.SocketContext.Connect();
            _heartBeatTimer.Change(0, 2000);
        }
        
        var result = await _context.EventContext.SendEvent<UinResolveEventResp>(new UinResolveEventReq(qid));
        if (result is { State: 0, Info: { } info })
        {
            _context.Keystore.Uin = info.Item1;
            _context.Keystore.State.Tlv104 = result.Tlv104;
            _context.LogInfo(Tag, $"Uin resolved: {info.Item1}, Qid: {info.Item2}");
            return info.Item1;
        }
        else if (result is { Error: { } error })
        {
            _context.LogError(Tag, $"Failed to resolve uin: {error.Item1} | {error.Item2}");
        }

        return 0;
    }

    public async Task<BotQrCodeInfo?> FetchQrCodeInfo(byte[] k)
    {
        var result = await _context.EventContext.SendEvent<VerifyCodeEventResp>(new VerifyCodeEventReq(k));
        if (result == null) return null;

        return new BotQrCodeInfo(result.Message, result.Platform, result.Location, result.Device);
    }

    public async Task<(bool, string)> CloseQrCode(byte[] k, bool confirm)
    {
        var result = await _context.EventContext.SendEvent<CloseCodeEventResp>(new CloseCodeEventReq(k, confirm));
        bool success = !confirm || result?.State == 0;
        return (success, result?.Message ?? string.Empty);
    }
    
    public bool SubmitCaptcha(string ticket, string randStr)
    {
        if (_captchaSource == null) return false;
        
        bool success = _captchaSource.TrySetResult((ticket, randStr));
        _captchaSource = null;
        return success;
    }

    public bool SubmitSMSCode(string code)
    {
        if (_smsSource == null) return false;

        bool success = _smsSource.TrySetResult(code);
        _smsSource = null;
        return success;
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

    private async Task<bool> KeyExchange()
    {
        var keyExchangeResult = await _context.EventContext.SendEvent<KeyExchangeEventResp>(new KeyExchangeEventReq());
        if (keyExchangeResult == null)
        {
            _context.LogError(Tag, "Key exchange failed");
            return false;
        }

        _context.Keystore.State.KeyExchangeSession = (keyExchangeResult.SessionTicket, keyExchangeResult.SessionKey);
        return true;
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
                    _context.EventInvoker.PostEvent(new BotRefreshKeystoreEvent(_context.Keystore));
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
    
    private void OnExchangeEmp(object? state) => Task.Run(async () =>
    {
        var result = await _context.EventContext.SendEvent<ExchangeEmpEventResp>(new ExchangeEmpEventReq(ExchangeEmpEventReq.Command.RefreshByA1));
        if (result == null) return;

        if (result.RetCode == 0)
        {
            ReadWLoginSigs(result.Tlvs);
            await Online();
            _context.EventInvoker.PostEvent(new BotRefreshKeystoreEvent(_context.Keystore));
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
                {
                    var reader = new BinaryPacket(value.AsSpan());
                    reader.Read<ushort>(); // FaceId
                    byte age = reader.Read<byte>();
                    byte gender = reader.Read<byte>();
                    string nickname = reader.ReadString(Prefix.Int8 | Prefix.LengthOnly);
                    _context.BotInfo = new BotInfo(age, gender, nickname);
                    break;
                }
                case 0x120:
                    _context.Keystore.WLoginSigs.SKey = value;
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
                case 0x512:
                {
                    _context.Keystore.WLoginSigs.PsKey.Clear();
                    
                    var reader = new BinaryPacket(value.AsSpan());
                    short domainCount = reader.Read<short>();
                    for (int i = 0; i < domainCount; i++)
                    {
                        string domain = reader.ReadString(Prefix.Int16 | Prefix.LengthOnly);
                        string key = reader.ReadString(Prefix.Int16 | Prefix.LengthOnly);
                        string pt4Token = reader.ReadString(Prefix.Int16 | Prefix.LengthOnly);
                        _context.Keystore.WLoginSigs.PsKey[domain] = key;
                    }
                    break;
                }
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
        _exchangeEmpTimer.Dispose();
    }
}