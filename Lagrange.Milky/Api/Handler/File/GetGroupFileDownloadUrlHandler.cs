using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;

namespace Lagrange.Milky.Api.Handler.File;

[Api("get_group_file_download_url")]
public class GetGroupFileDownloadUrlHandler(BotContext bot) : IApiHandler<GetGroupFileDownloadUrlParameter, GetGroupFileDownloadUrlResult>
{
    private readonly BotContext _bot = bot;

    public async Task<GetGroupFileDownloadUrlResult> HandleAsync(GetGroupFileDownloadUrlParameter parameter, CancellationToken token)
    {
        return new GetGroupFileDownloadUrlResult(await _bot.GroupFSDownload(parameter.GroupId, parameter.FileId));
    }
}

public class GetGroupFileDownloadUrlParameter(long groupId, string fileId)
{
    [JsonRequired]
    [JsonPropertyName("group_id")]
    public long GroupId { get; init; } = groupId;

    [JsonRequired]
    [JsonPropertyName("file_id")]
    public string FileId { get; init; } = fileId;
}

public class GetGroupFileDownloadUrlResult(string downloadUrl)
{
    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; } = downloadUrl;
}