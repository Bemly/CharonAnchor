using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;

namespace Lagrange.Milky.Implementation.Api.Handler.File;

[Api("delete_group_file")]
public class DeleteGroupFileApiHandler(BotContext bot) : IApiHandler<DeleteGroupFileApiParameter>
{
    private readonly BotContext _bot = bot;

    public async Task<IApiResult> HandleAsync(DeleteGroupFileApiParameter parameter, CancellationToken token)
    {
        await _bot.GroupFSDelete(parameter.GroupId, parameter.FileId);

        return IApiResult.Ok(new object { });
    }
}
