using Lagrange.Core;
using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Implementation.Utility;

public partial class Converter(ILogger<Converter> logger, BotContext bot)
{
    private readonly ILogger<Converter> _logger = logger;
    private readonly BotContext _bot = bot;
}