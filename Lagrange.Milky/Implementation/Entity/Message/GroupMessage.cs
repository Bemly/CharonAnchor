using System.Text.Json.Serialization;
using Lagrange.Milky.Implementation.Entity.Segment;

namespace Lagrange.Milky.Implementation.Entity.Message;

public class GroupMessage(long peerId, long messageSeq, long senderId, long time, IReadOnlyList<IIncomingSegment> segments, Group group, GroupMember groupMember) : MessageBase(peerId, messageSeq, senderId, time, segments, "group")
{
    [JsonPropertyName("group")]
    public Group Group { get; } = group;

    [JsonPropertyName("group_member")]
    public GroupMember GroupMember { get; } = groupMember;
}