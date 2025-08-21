using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;

namespace Lagrange.Core.Runner;

public class UrlSignProvider : BotSignProvider
{
    private static readonly HashSet<string> PcWhiteListCommand =
    [
        "trpc.o3.ecdh_access.EcdhAccess.SsoEstablishShareKey",
        "trpc.o3.ecdh_access.EcdhAccess.SsoSecureAccess",
        "trpc.o3.report.Report.SsoReport",
        "MessageSvc.PbSendMsg",
        "wtlogin.trans_emp",
        "wtlogin.login",
        "wtlogin.exchange_emp",
        "trpc.login.ecdh.EcdhService.SsoKeyExchange",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLogin",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginNewDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginEasyLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginPasswordLoginUnusualDevice",
        "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshTicket",
        "trpc.login.ecdh.EcdhService.SsoNTLoginRefreshA2",
        "OidbSvcTrpcTcp.0x11ec_1",
        "OidbSvcTrpcTcp.0x758_1", // create group
        "OidbSvcTrpcTcp.0x7c1_1",
        "OidbSvcTrpcTcp.0x7c2_5", // request friend
        "OidbSvcTrpcTcp.0x10db_1",
        "OidbSvcTrpcTcp.0x8a1_7", // request group
        "OidbSvcTrpcTcp.0x89a_0",
        "OidbSvcTrpcTcp.0x89a_15",
        "OidbSvcTrpcTcp.0x88d_0", // fetch group detail
        "OidbSvcTrpcTcp.0x88d_14",
        "OidbSvcTrpcTcp.0x112a_1",
        "OidbSvcTrpcTcp.0x587_74",
        "OidbSvcTrpcTcp.0x1100_1",
        "OidbSvcTrpcTcp.0x1102_1",
        "OidbSvcTrpcTcp.0x1103_1",
        "OidbSvcTrpcTcp.0x1107_1",
        "OidbSvcTrpcTcp.0x1105_1",
        "OidbSvcTrpcTcp.0xf88_1",
        "OidbSvcTrpcTcp.0xf89_1",
        "OidbSvcTrpcTcp.0xf57_1",
        "OidbSvcTrpcTcp.0xf57_106",
        "OidbSvcTrpcTcp.0xf57_9",
        "OidbSvcTrpcTcp.0xf55_1",
        "OidbSvcTrpcTcp.0xf67_1",
        "OidbSvcTrpcTcp.0xf67_5",
        "OidbSvcTrpcTcp.0x6d9_4"
    ];

    private readonly HttpClient _client = new();

    private readonly string _base = "https://sign.lagrangecore.org/api/sign";
    private readonly string _version = "30366";

    public override bool IsWhiteListCommand(string cmd) => PcWhiteListCommand.Contains(cmd);

    public override async Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        var response = await GetSign<PcSecSignRequest, PcSecSignResponse>(
            $"{_base}/{_version}",
            new PcSecSignRequest(cmd, seq, Convert.ToHexString(body.Span))
        );

        return new SsoSecureInfo
        {
            SecSign = Convert.FromHexString(response.Value.Sign),
            SecToken = Convert.FromHexString(response.Value.Token),
            SecExtra = Convert.FromHexString(response.Value.Extra)
        };
    }

    private async Task<TResponse> GetSign<TRequest, TResponse>(string url, TRequest requestJson) where TRequest : class where TResponse : class
    {
        using var request = new HttpRequestMessage();
        request.Method = HttpMethod.Post;
        request.RequestUri = new Uri(url);
        request.Content = JsonContent.Create(requestJson);
        using var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode) throw new Exception($"Unexpected http status code({response.StatusCode})");

        var result = JsonSerializer.Deserialize<TResponse>(await response.Content.ReadAsStreamAsync());
        if (result == null) throw new NullReferenceException("Result is null");

        return result;
    }
}

public class PcSecSignRequest(string cmd, int seq, string src)
{
    [JsonPropertyName("cmd")]
    public string Cmd { get; } = cmd;

    [JsonPropertyName("seq")]
    public int Seq { get; } = seq;

    [JsonPropertyName("src")]
    public string Src { get; } = src;
}

public class PcSecSignResponse(PcSecSignResponseValue value)
{
    [JsonRequired]
    [JsonPropertyName("value")]
    public PcSecSignResponseValue Value { get; init; } = value;
}

public class PcSecSignResponseValue(string sign, string token, string extra)
{
    [JsonRequired]
    [JsonPropertyName("sign")]
    public string Sign { get; init; } = sign;

    [JsonRequired]
    [JsonPropertyName("token")]
    public string Token { get; init; } = token;

    [JsonRequired]
    [JsonPropertyName("extra")]
    public string Extra { get; init; } = extra;
}