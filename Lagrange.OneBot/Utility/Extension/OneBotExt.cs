using Lagrange.Core.Common;

namespace Lagrange.OneBot.Utility.Extension;

public static class GenderExt
{
    public static string ToOneBotString(this BotGender info)
    {
        return info switch
        {
            BotGender.Male => "male",
            BotGender.Female => "female",
            _ => "unknown"
        };
    }
}
