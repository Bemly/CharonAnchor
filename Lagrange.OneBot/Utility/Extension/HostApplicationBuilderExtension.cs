using System.Data.Common;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.OneBot.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lagrange.OneBot.Utility.Extension;

public static class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureCore(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<BotContext>(_ => BotFactory.Create(new BotConfig
        {
            UseIPv6Network = false 
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
    
    public static HostApplicationBuilder ConfigureServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<StorageService>();
        return builder;
    }
}