using Lagrange.Core;
using Lagrange.Milky.Cache;

namespace Lagrange.Milky.Utility;

public partial class EntityConvert(BotContext bot, MessageCache cache, ResourceResolver resolver)
{
    private readonly BotContext _bot = bot;
    private readonly MessageCache _cache = cache;
    private readonly ResourceResolver _resolver = resolver;
}