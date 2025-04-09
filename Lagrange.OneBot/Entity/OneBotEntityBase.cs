using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Entity;

[Serializable]
public abstract class OneBotEntityBase(long selfId, string postType, long time)
{
    public OneBotEntityBase(long selfId, string postType)
        : this(selfId, postType, DateTimeOffset.Now.ToUnixTimeSeconds())
    {
    }

    [JsonPropertyName("time")] public long Time { get; set; } = time;

    [JsonPropertyName("self_id")] public long SelfId { get; set; } = selfId;

    [JsonPropertyName("post_type")] public string PostType { get; set; } = postType;
}