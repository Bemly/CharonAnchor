using System.Reflection;
using Lagrange.Core;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Network;
using Lagrange.OneBot.Database;
using Lagrange.OneBot.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Lagrange.OneBot.Message;

namespace Lagrange.OneBot.Operation;

public sealed class OperationService
{
    private readonly BotContext _bot;
    private readonly ILogger _logger;
    private readonly Dictionary<string, Type> _operations;
    private readonly ServiceProvider _service;

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public OperationService(BotContext bot, ILogger<OperationService> logger, StorageService storage, MessageService message)
    {
        _bot = bot;
        _logger = logger;

        _operations = new Dictionary<string, Type>();
        foreach (var type in typeof(OperationService).Assembly.GetTypes())
        {
            var attributes = type.GetCustomAttributes<OperationAttribute>();
            foreach (var attribute in attributes) _operations[attribute.Api] = type;
        }

        var service = new ServiceCollection();
        service.AddSingleton(bot);
        service.AddSingleton(logger);
        service.AddSingleton(storage);
        service.AddSingleton(message);
        service.AddLogging();

        foreach (var (_, type) in _operations) service.AddScoped(type);
        _service = service.BuildServiceProvider();
    }

    public async Task<OneBotResult?> HandleOperation(MsgRecvEventArgs e)
    {
        try
        {
            if (JsonHelper.Deserialize<OneBotAction>(e.Data) is { } action)
            {
                try
                {
                    if (_operations.TryGetValue(action.Action, out var type))
                    {
                        var handler = (IOperation)_service.GetRequiredService(type);
                        var result = await handler.HandleOperation(_bot, action.Params);
                        result.Echo = action.Echo;

                        return result;
                    }

                    return new OneBotResult(null, 404, "failed") { Echo = action.Echo };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unexpected error encountered while handling message.");
                    return new OneBotResult(null, 200, "failed") { Echo = action.Echo };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Json Serialization failed for such action");
        }

        return null;
    }
}