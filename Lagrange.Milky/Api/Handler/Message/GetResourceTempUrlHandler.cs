
using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("get_resource_temp_url")]
public class GetResourceTempUrlHandler(BotContext bot) : IApiHandler<GetResourceTempUrlParameter, GetResourceTempUrlResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetResourceTempUrlResult> HandleAsync(GetResourceTempUrlParameter parameter, CancellationToken token)
    {
        return new GetResourceTempUrlResult(await _bot.GetNTV2RichMediaUrl(parameter.ResourceId));
    }
}

public class GetResourceTempUrlParameter(string resourceId)
{
    [JsonRequired]
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; init; } = resourceId;
}

public class GetResourceTempUrlResult(string url)
{
    [JsonPropertyName("url")]
    public string Url { get; } = url;
}