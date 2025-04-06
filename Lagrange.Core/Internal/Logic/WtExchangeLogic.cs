using Lagrange.Core.Internal.Packets.Login;

namespace Lagrange.Core.Internal.Logic;

internal class WtExchangeLogic : ILogic, IDisposable
{
    private const string Tag = nameof(WtExchangeLogic);
    
    private readonly BotContext _context;
    
    private readonly Timer _heartBeatTimer;

    private readonly Timer _ssoHeartBeatTimer;

    public WtExchangeLogic(BotContext context)
    {
        _context = context;
        _heartBeatTimer = new Timer(OnHeartBeat);
        _ssoHeartBeatTimer = new Timer(OnSsoHeartBeat);
    }

    public async Task<bool> Login(string? password)
    {
        if (!_context.SocketContext.Connected) await _context.SocketContext.Connect();

        if (_context.Keystore.WLoginSigs.D2.Length != 0)
        {
            bool online = await Online();
            if (online) return true;
        }

        return await ManualLogin(password);
    }

    private async Task<bool> ManualLogin(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            var transEmp31 = new WtLogin(_context).BuildTransEmp31();
            _context.LogInfo(Tag, Convert.ToHexString(transEmp31.ToArray()));
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

    private void OnHeartBeat(object? state) => Task.Run(() =>
    {
        
    });
    
    private void OnSsoHeartBeat(object? state) => Task.Run(() =>
    {
        
    });

    public void Dispose()
    {
        _heartBeatTimer.Dispose();
        _ssoHeartBeatTimer.Dispose();
    }
}