using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Entity.Meta;

[Serializable]
public class OneBotHeartBeat(long selfId, int interval, object status) : OneBotMeta(selfId, "heartbeat")
{
    [JsonPropertyName("interval")] public int Interval { get; set; } = interval;

    [JsonPropertyName("status")] public object Status { get; set; } = status;
}