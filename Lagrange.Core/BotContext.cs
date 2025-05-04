using Lagrange.Core.Common;
using Lagrange.Core.Events;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Context;
using Lagrange.Core.Message;

namespace Lagrange.Core;

public class BotContext : IDisposable
{
    internal BotContext(BotConfig config, BotKeystore keystore, BotAppInfo appInfo)
    {
        Config = config;
        AppInfo = appInfo;
        Keystore = keystore;
        
        EventInvoker = new EventInvoker(this);
        
        CacheContext = new CacheContext(this);
        PacketContext = new PacketContext(this);
        ServiceContext = new ServiceContext(this);
        SocketContext = new SocketContext(this);
        EventContext = new EventContext(this);
    }

    public BotConfig Config { get; }
    public BotAppInfo AppInfo { get; }
    public BotKeystore Keystore { get; }
    public long BotUin => Keystore.Uin;
    public BotInfo? BotInfo { get; internal set; }
    
    
    public bool IsOnline { get; internal set; }
    public EventInvoker EventInvoker { get; }
    
    internal CacheContext CacheContext { get; }
    internal PacketContext PacketContext { get; }
    internal ServiceContext ServiceContext { get; }
    internal SocketContext SocketContext { get; }
    internal EventContext EventContext { get; }

    #region Shortcut Methods

    public void LogCritical(string tag, string message) => EventInvoker.PostEvent(new BotLogEvent(tag, LogLevel.Critical, message));
    
    public void LogError(string tag, string message) => EventInvoker.PostEvent(new BotLogEvent(tag, LogLevel.Error, message));
    
    public void LogWarning(string tag, string message) => EventInvoker.PostEvent(new BotLogEvent(tag, LogLevel.Warning, message));
    
    public void LogInfo(string tag, string message) => EventInvoker.PostEvent(new BotLogEvent(tag, LogLevel.Information, message));
    
    public void LogDebug(string tag, string message) => EventInvoker.PostEvent(new BotLogEvent(tag, LogLevel.Debug, message));
    
    public void LogTrace(string tag, string message) => EventInvoker.PostEvent(new BotLogEvent(tag, LogLevel.Trace, message));

    #endregion

    public void Dispose()
    {
        EventInvoker.Dispose();
        SocketContext.Dispose();
        EventContext.Dispose();
    }
}