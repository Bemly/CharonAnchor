using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Implementation.Api.Handler.File;

[Api("delete_group_file")]
public class DeleteGroupFileHandler(BotContext bot) : IApiHandler<DeleteGroupFileParameter, object>
{
    private readonly BotContext _bot = bot;

    public async Task<object> HandleAsync(DeleteGroupFileParameter parameter, CancellationToken token)
    {
        await _bot.GroupFSDelete(parameter.GroupId, parameter.FileId);

        return new object();
    }
}

public class DeleteGroupFileParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("file_id")]
    public required string FileId { get; init; }
}