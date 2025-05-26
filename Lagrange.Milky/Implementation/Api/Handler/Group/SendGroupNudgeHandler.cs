using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Implementation.Api.Handler.Group;

[Api("send_group_nudge")]
public class SendGroupNudgeHandler(BotContext bot) : IApiHandler<SendGroupNudgeParameter, object>
{
    private readonly BotContext _bot = bot;

    public async Task<object> HandleAsync(SendGroupNudgeParameter parameter, CancellationToken token)
    {
        await _bot.SendGroupNudge(parameter.GroupId, parameter.UserId);

        return new object();
    }
}

public class SendGroupNudgeParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("user_id")]
    public required long UserId { get; init; }
}