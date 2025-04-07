using Lagrange.Core.Internal.Events.Login;
using Lagrange.Core.Internal.Events.System;

namespace Lagrange.Core.Internal.Logic;

internal class WtExchangeLogic : ILogic, IDisposable
{
    private const string Tag = nameof(WtExchangeLogic);
    
    private readonly BotContext _context;
    
    private readonly Timer _heartBeatTimer;

    private readonly Timer _ssoHeartBeatTimer;

    private readonly Timer _queryStateTimer;
    
    private readonly TaskCompletionSource<bool> _tcs = new();

    public WtExchangeLogic(BotContext context)
    {
        _context = context;
        _heartBeatTimer = new Timer(OnHeartBeat);
        _ssoHeartBeatTimer = new Timer(OnSsoHeartBeat);
        _queryStateTimer = new Timer(OnQueryState);
    }

    public async Task<bool> Login(long uin, string? password)
    {
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
            
            _context.Keystore.WLoginSigs.QrSig = transEmp31.QrSig;
            _queryStateTimer.Change(0, 2000);
        }
        else
        {
            
        }
        
        return false;
    }

    private async Task<bool> Online()
    {
        return false;
    }

    private void OnHeartBeat(object? state) => Task.Run(async () =>
    {
        await _context.EventContext.SendEvent<AliveEvent>(new AliveEvent());
    });
    
    private void OnSsoHeartBeat(object? state) => Task.Run(() =>
    {
        
    });
    
    private void OnQueryState(object? state) => Task.Run(async () =>
    {
        var transEmp12 = await _context.EventContext.SendEvent<TransEmp12EventResp>(new TransEmp12EventReq());
        switch (transEmp12)
        {
            case null:
                return;
            case { State: TransEmp12EventResp.TransEmpState.Confirmed, Data: { } data }:
                _context.Keystore.WLoginSigs.TgtgtKey = data.TgtgtKey;
                _context.Keystore.WLoginSigs.NoPicSig = data.NoPicSig;
                _context.Keystore.WLoginSigs.EncryptedA1 = data.TempPassword;
            
                _tcs.TrySetResult(true);
                _queryStateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                break;
            case { State: TransEmp12EventResp.TransEmpState.Canceled or TransEmp12EventResp.TransEmpState.Invalid or TransEmp12EventResp.TransEmpState.CodeExpired }:
                _context.LogCritical(Tag, $"QR Code State: {transEmp12.State}");
                
                _tcs.TrySetResult(false);
                _queryStateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                break;
            default:
                _context.LogInfo(Tag, $"QRCode State: {transEmp12.State}");
                break;
        }

    });

    public void Dispose()
    {
        _heartBeatTimer.Dispose();
        _ssoHeartBeatTimer.Dispose();
        _queryStateTimer.Dispose();
    }
}