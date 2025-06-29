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
        var (data, command, sequence, retCode, extra) = await _bot.SendPacket(
            parameter.Cmd,
            Convert.FromHexString(parameter.Payload),
            parameter.Seq
        );

        return new SendPacketResult(Convert.ToHexString(data.Span), command, extra, retCode, sequence);
    }
}

public class SendPacketParameter(string cmd, int seq, string payload)
{
    [JsonRequired]
    [JsonPropertyName("cmd")]
    public string Cmd { get; init; } = cmd;

    [JsonRequired]
    [JsonPropertyName("seq")]
    public int Seq { get; init; } = seq;

    [JsonRequired]
    [JsonPropertyName("payload")]
    public string Payload { get; init; } = payload;
}

public class SendPacketResult(string data, string command, string extra, int retCode, int sequence)
{
    [JsonPropertyName("data")]
    public string Data { get; } = data;

    [JsonPropertyName("command")]
    public string Command { get; } = command;

    [JsonPropertyName("extra")]
    public string Extra { get; } = extra;

    [JsonPropertyName("ret_code")]
    public int RetCode { get; } = retCode;

    [JsonPropertyName("sequence")]
    public int Sequence { get; } = sequence;
}