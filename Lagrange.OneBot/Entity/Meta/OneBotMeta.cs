using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Entity.Meta;

[Serializable]
public abstract class OneBotMeta(long selfId, string metaEventType) : OneBotEntityBase(selfId, "meta_event")
{
    [JsonPropertyName("meta_event_type")] public string MetaEventType { get; set; } = metaEventType;
}