using Lagrange.Core;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;

namespace Lagrange.Milky.Implementation.Api.Handler.File;

[Api("move_group_file")]
public class MoveGroupFileApiHandler(BotContext bot) : IApiHandler<MoveGroupFileApiParameter>
{
    private readonly BotContext _bot = bot;

    public Task<IApiResult> HandleAsync(MoveGroupFileApiParameter parameter, CancellationToken token)
    {
        throw new NotImplementedException();
        // var parent = await _bot.GroupFSGetFolderId

        // return IApiResult.Ok(new GetGroupFileDownloadUrlResultData
        // {
        //     DownloadUrl = await _bot.GroupFSMove(parameter.GroupId, parameter.FileId, parameter.TargetFolderId, parent)
        // });
    }
}
