using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;
using Lagrange.Milky.Implementation.Api.Result.Data;

namespace Lagrange.Milky.Implementation.Api.Handler.File;

[Api("get_group_file_download_url")]
public class GetGroupFileDownloadUrlApiHandler(BotContext bot) : IApiHandler<GetGroupFileDownloadUrlApiParameter>
{
    private readonly BotContext _bot = bot;

    public async Task<IApiResult> HandleAsync(GetGroupFileDownloadUrlApiParameter parameter, CancellationToken token)
    {
        return IApiResult.Ok(new GetGroupFileDownloadUrlResultData
        {
            DownloadUrl = await _bot.GroupFSDownload(parameter.GroupId, parameter.FileId)
        });
    }
}
