using System.Text.Json.Serialization;
using Lagrange.Core.Common.Entity;
using Lagrange.OneBot.Utility.Extension;

namespace Lagrange.OneBot.Entity.Message;

[Serializable]
public class OneBotGroupMessage(long selfId, long groupUin, List<OneBotSegment> message, string rawMessage, BotGroupMember member, int messageId, long time) : OneBotEntityBase(selfId, "message", time)
{
    [JsonPropertyName("message_type")] public string MessageType { get; set; } = "group";

    [JsonPropertyName("sub_type")] public string SubType { get; set; } = "normal";

    [JsonPropertyName("message_id")] public int MessageId { get; set; } = messageId;

    [JsonPropertyName("group_id")] public long GroupId { get; set; } = groupUin;
    
    [JsonPropertyName("user_id")] public long UserId { get; set; } = member.Uin;
    
    [JsonPropertyName("anonymous")] public object? Anonymous { get; set; }

    [JsonPropertyName("message")] public List<OneBotSegment> Message { get; set; } = message;

    [JsonPropertyName("raw_message")] public string RawMessage { get; set; } = rawMessage;

    [JsonPropertyName("font")] public int Font { get; set; } = 0;

    [JsonPropertyName("sender")] public OneBotGroupSender GroupSender { get; set; } = new(member.Uin, member.Nickname, member.MemberCard ?? string.Empty, member.GroupLevel, member.Permission)
    {
        Age = member.Age,
        Sex = member.Gender.ToOneBotString()
    };
}