using System.Data.Common;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.OneBot.Core;
using Lagrange.OneBot.Network;
using Lagrange.OneBot.Network.Service;
using Lagrange.OneBot.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.OneBot.Utility.Extension;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureCore(this HostApplicationBuilder builder)
    {
        var option = new AccountOption();
        builder.Configuration.GetSection("Account").Bind(option);
        
        builder.Services.AddSingleton<BotContext>(_ => BotFactory.Create(new BotConfig
        {
            UseIPv6Network = option.UseIPv6Network,
            GetOptimumServer = option.GetOptimumServer,
            AutoReconnect = option.AutoReconnect,
            Protocol = option.Protocol,
            AutoReLogin = option.AutoReLogin
        }));
        
        return builder;
    }
    
    public static HostApplicationBuilder ConfigureSQLite(this HostApplicationBuilder builder)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(builder.Configuration["DatabasePath"] ?? "lagrange.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = false,
            Cache = SqliteCacheMode.Shared,
            Pooling = true
        };
        builder.Services.AddSingleton<DbConnection, SqliteConnection>(_ =>
        {
            var conn = new SqliteConnection(connectionString.ToString());
            conn.Open();
            return conn;
        });
        return builder;
    }

    public static HostApplicationBuilder ConfigureOneBot(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddSingleton<LagrangeWebSvcCollection>()

            .AddScoped<ILagrangeWebServiceFactory<ForwardWSService>, ForwardWSServiceFactory>()
            .AddScoped<ForwardWSService>()

            .AddScoped<ILagrangeWebServiceFactory<ReverseWSService>, ReverseWSServiceFactory>()
            .AddScoped<ReverseWSService>()

            .AddScoped<ILagrangeWebServiceFactory<HttpService>, HttpServiceFactory>()
            .AddScoped<HttpService>()

            .AddScoped<ILagrangeWebServiceFactory<HttpPostService>, HttpPostServiceFactory>()
            .AddScoped<HttpPostService>()

            .AddScoped<ILagrangeWebServiceFactory, DefaultLagrangeWebServiceFactory>()
            .AddScoped(services => services.GetRequiredService<ILagrangeWebServiceFactory>().Create() ?? throw new Exception("Invalid conf detected"));
        
        return builder;
    }
    
    public static HostApplicationBuilder ConfigureServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<StorageService>();
        return builder;
    }
}