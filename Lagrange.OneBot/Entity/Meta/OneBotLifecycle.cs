using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Entity.Meta;

[Serializable]
public class OneBotLifecycle(long selfId, string subType) : OneBotMeta(selfId, "lifecycle")
{
    [JsonPropertyName("sub_type")] public string SubType { get; set; } = subType;
}