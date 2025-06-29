using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("delete_group_file")]
public class DeleteGroupFileHandler(BotContext bot) : IEmptyResultApiHandler<DeleteGroupFileParameter>
{
    private readonly BotContext _bot = bot;

    public async Task HandleAsync(DeleteGroupFileParameter parameter, CancellationToken token)
    {
        await _bot.GroupFSDelete(parameter.GroupId, parameter.FileId);
    }
}

public class DeleteGroupFileParameter(long groupId, string fileId)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("file_id")]
    public string FileId { get; init; } = fileId;
}