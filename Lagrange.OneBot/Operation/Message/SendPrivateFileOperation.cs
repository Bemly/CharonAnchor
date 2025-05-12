using System.Text.Json.Nodes;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Utility;

namespace Lagrange.OneBot.Operation.Message;

[Operation("upload_private_file")]
public class SendPrivateFileOperation : IOperation
{
    public async Task<OneBotResult> HandleOperation(BotContext context, JsonNode? payload)
    {
        if (payload.Deserialize<OneBotUploadPrivateFile>() is not { } file) throw new Exception();

        var stream = new FileStream(file.File, FileMode.Open);
        try
        {
            await context.SendFriendFile(file.UserId, stream, file.Name);
            return new OneBotResult(null, 200, "ok");
        }
        catch (Exception e)
        {
            return new OneBotResult(e.Message, 521, "error");
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }
}