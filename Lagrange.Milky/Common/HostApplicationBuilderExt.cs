using System.Text;
using System.Text.Json;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Common.Config;
using Lagrange.Milky.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky.Common;

public static class HostApplicationBuilderExt
{
    public static HostApplicationBuilder UseJsonFileWithComments(
        this HostApplicationBuilder builder,
        string path)
    {
        string json = File.ReadAllText(path);
        var jsonDocument = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });
        string normalizedJson = jsonDocument.RootElement.ToString();
        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(normalizedJson)));
        return builder;
    }

    public static HostApplicationBuilder ConfigureCore(this HostApplicationBuilder builder)
    {
        var accountConfig = new AccountConfig();
        builder.Configuration.GetSection("Account").Bind(accountConfig);
        builder.Services.Configure<AccountConfig>(builder.Configuration.GetSection("Account"));

        var botConfig = new BotConfig
        {
            UseIPv6Network = accountConfig.UseIPv6Network,
            GetOptimumServer = accountConfig.GetOptimumServer,
            AutoReconnect = accountConfig.AutoReconnect,
            Protocol = accountConfig.Protocol,
            AutoReLogin = accountConfig.AutoReLogin
        };

        string? file = Directory.GetFiles(".").FirstOrDefault(f => f.EndsWith(".keystore"));
        if (file != null && JsonHelper.Deserialize<BotKeystore>(File.ReadAllText(file)) is { } keystore)
        {
            builder.Services.AddSingleton<BotContext>(_ => BotFactory.Create(botConfig, keystore));
        }
        else
        {
            builder.Services.AddSingleton<BotContext>(_ => BotFactory.Create(botConfig));
        }

        builder.Services.AddHostedService<BotService>();
        builder.Services.AddSingleton<ICaptchaResolver, OnlineCaptchaResolver>();
        return builder;
    }
}