using System.Text.Json.Serialization;
using Lagrange.Core;
using Lagrange.Core.Common;

namespace Lagrange.Milky.Api.Handler.System;

[Api("get_impl_info")]
public class GetImplInfoHandler(BotContext bot) : IEmptyParameterApiHandler<GetImplInfoResult>
{
    private readonly BotContext _bot = bot;

    public Task<GetImplInfoResult> HandleAsync(CancellationToken token)
    {
        return Task.FromResult(new GetImplInfoResult(
            Constants.ImplementationName,
            Constants.ImplementationVersion,
            _bot.AppInfo.CurrentVersion,
            _bot.Config.Protocol switch
            {
                Protocols.Windows => "windows",
                Protocols.MacOs => "macos",
                Protocols.Linux => "linux",
                Protocols.AndroidPhone => "android_phone",
                Protocols.AndroidPad => "android_pad",
                Protocols.AndroidWatch => "watch",
                _ => throw new NotSupportedException(),
            },
            Constants.MilkyVersion
        ));
    }
}

public class GetImplInfoResult(string implName, string implVersion, string qqProtocolVersion, string qqProtocolType, string milkyVersion)
{
    [JsonPropertyName("impl_name")]
    public string ImplName { get; } = implName;

    [JsonPropertyName("impl_version")]
    public string ImplVersion { get; } = implVersion;

    [JsonPropertyName("qq_protocol_version")]
    public string QqProtocolVersion { get; } = qqProtocolVersion;

    [JsonPropertyName("qq_protocol_type")]
    public string QqProtocolType { get; } = qqProtocolType;

    [JsonPropertyName("milky_version")]
    public string MilkyVersion { get; } = milkyVersion;
}