namespace Lagrange.Core.Common.Interface;

public static class BotFactory
{
    /// <summary>
    /// Create new Bot from existing <see cref="BotKeystore"/>
    /// </summary>
    /// <param name="config">The config for Bot</param>
    /// <param name="keystore">Existing Keystore from deserialization</param>
    /// <param name="appInfo">The app info for Bot, if null, will use default app info from <see cref="BotAppInfo.ProtocolToAppInfo"/></param>
    /// <returns>Created BotContext Instance</returns>
    public static BotContext Create(BotConfig config, BotKeystore keystore, BotAppInfo? appInfo = null) =>
        new(config, keystore, appInfo ?? BotAppInfo.ProtocolToAppInfo[config.Protocol]);
    
    /// <summary>
    /// Create new Bot from Empty
    /// </summary>
    /// <param name="config">The config for Bot</param>
    /// <param name="appInfo">The app info for Bot, if null, will use default app info from <see cref="BotAppInfo.ProtocolToAppInfo"/></param>
    /// <returns>Created BotContext Instance</returns>
    public static BotContext Create(BotConfig config, BotAppInfo? appInfo = null) =>
        new(config, BotKeystore.CreateEmpty(), appInfo ?? BotAppInfo.ProtocolToAppInfo[config.Protocol]);
}