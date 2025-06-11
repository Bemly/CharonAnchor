using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Entity.Segment;

namespace Lagrange.Milky.Implementation.Entity.Message;

public class FriendMessage(long peerId, long messageSeq, long senderId, long time, IReadOnlyList<IIncomingSegment> segments, Friend friend, long clientSeq) : MessageBase(peerId, messageSeq, senderId, time, segments, "friend")
{
    [JsonPropertyName("friend")]
    public Friend Friend { get; } = friend;

    [JsonPropertyName("client_seq")]
    public long ClientSeq { get; } = clientSeq;
}