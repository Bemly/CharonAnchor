using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.Group;

[Api("set_group_member_card")]
public class SetGroupMemberCardHandler(BotContext bot) : IEmptyResultApiHandler<SetGroupMemberCardParameter>
{
    private readonly BotContext _bot = bot;

    public async Task HandleAsync(SetGroupMemberCardParameter parameter, CancellationToken token)
    {
        await _bot.GroupMemberRename(parameter.GroupId, parameter.UserId, parameter.Card);
    }
}

public class SetGroupMemberCardParameter(long groupId, long userId, string card)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("user_id")]
    public long UserId { get; init; } = userId;

    [JsonRequired]
    [JsonPropertyName("card")]
    public string Card { get; init; } = card;
}
