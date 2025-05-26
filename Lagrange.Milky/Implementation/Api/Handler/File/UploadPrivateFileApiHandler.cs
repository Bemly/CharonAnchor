using Lagrange.Core;
using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;

namespace Lagrange.Milky.Implementation.Api.Handler.File;

[Api("upload_private_file")]
public class UploadPrivateFileApiHandler(BotContext bot) : IApiHandler<UploadPrivateFileApiParameter>
{
    private readonly BotContext _bot = bot;

    public Task<IApiResult> HandleAsync(UploadPrivateFileApiParameter parameter, CancellationToken token)
    {
        throw new NotImplementedException();
        // await _bot.SendFriendFile(parameter.UserId, await UriUtility.ToMemoryStreamAsync(parameter.FileUri, token));
    }
}
