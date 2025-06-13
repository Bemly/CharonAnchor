using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Extension;

public static class LoggerFilterOptionsExtension
{
    public static LogLevel GetDefaultLogLevel(this LoggerFilterOptions options)
    {
        return options.Rules.FirstOrDefault(rule =>
            {
                return rule.ProviderName == null
                    && rule.CategoryName == null
                    && rule.Filter == null;
            })
            ?.LogLevel
            ?? options.MinLevel;
    }
}