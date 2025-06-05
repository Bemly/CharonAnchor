
using System.Text.Json.Serialization;
using Lagrange.Core;

namespace Lagrange.Milky.Implementation.Api.Handler.System;

[Api("get_impl_info")]
public class GetImplInfoHandler(BotContext bot) : IApiHandler<object, GetImplInfoResult>
{
    private readonly BotContext _bot = bot;

    public Task<GetImplInfoResult> HandleAsync(object parameter, CancellationToken token)
    {
        return Task.FromResult(new GetImplInfoResult
        {
            ImplName = Constants.ImplementationName,
            ImplVersion = Constants.ImplementationVersion,
            QqProtocolVersion = _bot.AppInfo.CurrentVersion,
            QqProtocolType = _bot.Config.Protocol switch
            {
                Lagrange.Core.Common.Protocols.Windows => "windows",
                Lagrange.Core.Common.Protocols.MacOs => "macos",
                Lagrange.Core.Common.Protocols.Linux => "linux",
                Lagrange.Core.Common.Protocols.AndroidPhone => "android_phone",
                Lagrange.Core.Common.Protocols.AndroidPad => "android_pad",
                _ => throw new NotSupportedException(),
            },
            MilkyVersion = Constants.MilkyVersion,
        });
    }
}

public class GetImplInfoResult
{
    [JsonPropertyName("impl_name")]
    public required string ImplName { get; init; }

    [JsonPropertyName("impl_version")]
    public required string ImplVersion { get; init; }

    [JsonPropertyName("qq_protocol_version")]
    public required string QqProtocolVersion { get; init; }

    [JsonPropertyName("qq_protocol_type")]
    public required string QqProtocolType { get; init; }

    [JsonPropertyName("milky_version")]
    public required string MilkyVersion { get; init; }
}