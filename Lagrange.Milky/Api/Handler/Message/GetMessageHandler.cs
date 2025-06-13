using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common.Interface;
using Lagrange.Milky.Api.Exception;
using Lagrange.Milky.Entity.Message;
using Lagrange.Milky.Utility;

namespace Lagrange.Milky.Api.Handler.Message;

[Api("get_message")]
public class GetMessageHandler(BotContext bot, EntityConvert convert) : IApiHandler<GetMessageParameter, GetMessageResult>
{
    private readonly BotContext _bot = bot;
    private readonly EntityConvert _convert = convert;

    public async Task<GetMessageResult> HandleAsync(GetMessageParameter parameter, CancellationToken token)
    {
        int sequence = (int)parameter.MessageSeq;
        var messages = parameter.MessageScene switch
        {
            "friend" => throw new NotImplementedException(),
            "group" => await _bot.GetGroupMessage(parameter.PeerId, sequence, sequence),
            "temp" => throw new ApiException(-1, "temp not supported"),
            _ => throw new NotSupportedException(),
        };

        if (messages.Count == 0) throw new ApiException(-2, "message not found");

        return new GetMessageResult(_convert.MessageBase(messages[0]));
    }
}

public class GetMessageParameter(string messageScene, long peerId, long messageSeq)
{
    [JsonRequired]
    [JsonPropertyName("message_scene")]
    public string MessageScene { get; init; } = messageScene;

    [JsonRequired]
    [JsonPropertyName("peer_id")]
    public long PeerId { get; init; } = peerId;

    [JsonRequired]
    [JsonPropertyName("message_seq")]
    public long MessageSeq { get; init; } = messageSeq;
}

public class GetMessageResult(MessageBase message)
{
    [JsonPropertyName("message")]
    public MessageBase Message { get; init; } = message;
}