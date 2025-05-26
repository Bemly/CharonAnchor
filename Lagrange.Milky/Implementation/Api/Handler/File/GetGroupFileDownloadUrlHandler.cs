using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Implementation.Api.Handler.File;

[Api("get_group_file_download_url")]
public class GetGroupFileDownloadUrlHandler(BotContext bot) : IApiHandler<GetGroupFileDownloadUrlParameter, GetGroupFileDownloadUrlResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetGroupFileDownloadUrlResult> HandleAsync(GetGroupFileDownloadUrlParameter parameter, CancellationToken token)
    {
        return new GetGroupFileDownloadUrlResult
        {
            DownloadUrl = await _bot.GroupFSDownload(parameter.GroupId, parameter.FileId)
        };
    }
}

public class GetGroupFileDownloadUrlParameter
{
    [JsonPropertyName("group_id")]
    public required long GroupId { get; init; }

    [JsonPropertyName("file_id")]
    public required string FileId { get; init; }
}

public class GetGroupFileDownloadUrlResult
{
    [JsonPropertyName("download_url")]
    public required string DownloadUrl { get; init; }
}