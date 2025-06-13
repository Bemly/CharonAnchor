using System.Text.Json.Serialization;
using Lagrange.Milky.Entity.Segment;

namespace Lagrange.Milky.Entity.Message;

public class TempMessage(long peerId, long messageSeq, long senderId, long time, IReadOnlyList<IIncomingSegment> segments, Group? group = null) : MessageBase(peerId, messageSeq, senderId, time, segments, "temp")
{
    [JsonPropertyName("group")]
    public Group? Group { get; } = group;
}