using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Milky.Extension;

namespace Lagrange.Milky.Api.Handler.Debug;

[Api("_send_packet", true)]
public class SendPacketHandler(BotContext bot) : IApiHandler<SendPacketParameter, SendPacketResult>
{
    private readonly BotContext _bot = bot;

    public async Task<SendPacketResult> HandleAsync(SendPacketParameter parameter, CancellationToken token)
    {
        var (retCode, extra, data) = await _bot.SendPacket(
            parameter.Cmd,
            parameter.Seq,
            Convert.FromHexString(parameter.Payload)
        );

        return new SendPacketResult(retCode, extra, Convert.ToHexString(data.Span));
    }
}

public class SendPacketParameter(string cmd, int seq, string payload)
{
    [JsonRequired]
    [JsonPropertyName("cmd")]
    public string Cmd { get; init; } = cmd;

    [JsonRequired]
    [JsonPropertyName("sequence")]
    public int Seq { get; init; } = seq;

    [JsonRequired]
    [JsonPropertyName("data")]
    public string Payload { get; init; } = payload;
}

public class SendPacketResult(int retCode, string extra, string data)
{
    [JsonPropertyName("ret_code")]
    public int RetCode { get; } = retCode;

    [JsonPropertyName("extra")]
    public string Extra { get; } = extra;

    [JsonPropertyName("data")]
    public string Data { get; } = data;
}