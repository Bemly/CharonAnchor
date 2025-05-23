using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Implementation.Entity;
using Lagrange.Milky.Implementation.Entity.Incoming.Message;
using Lagrange.Milky.Implementation.Entity.Incoming.Segment;
using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Implementation.Utility;

public class EntityConvert(ILogger<EntityConvert> logger)
{
    private readonly ILogger<EntityConvert> _logger = logger;

    public Friend Friend(BotFriend friend) => new()
    {
        UserId = friend.Uin,
        Qid = friend.Qid,
        Nickname = friend.Nickname,
        Remark = friend.Remarks,
        Category = FriendCategory(friend.Category),
    };

    public FriendCategory FriendCategory(BotFriendCategory category) => new()
    {
        CategoryId = category.Id,
        CategoryName = category.Name,
    };

    public Group Group(BotGroup group) => new()
    {
        GroupId = group.Uin,
        Name = group.GroupName,
        MemberCount = group.MemberCount,
        MaxMemberCount = group.MaxMember,
    };

    public GroupMember GroupMember(BotGroupMember member) => new()
    {
        GroupId = member.Group.Uin,
        UserId = member.Uin,
        Nickname = member.Nickname,
        Card = member.MemberCard ?? string.Empty,
        Title = member.SpecialTitle,
        Sex = member.Gender switch
        {
            BotGender.Male => "male",
            BotGender.Female => "female",
            BotGender.Unset or
            BotGender.Unknown => "unknown",
            _ => throw new NotSupportedException(),
        },
        Level = member.GroupLevel,
        Role = member.Permission switch
        {
            GroupMemberPermission.Member => "member",
            GroupMemberPermission.Admin => "admin",
            GroupMemberPermission.Owner => "owner",
            _ => throw new NotImplementedException(),
        },
        JoinTime = new DateTimeOffset(member.JoinTime).ToUnixTimeSeconds(),
        LastSentTime = new DateTimeOffset(member.LastMsgTime).ToUnixTimeSeconds(),
    };

    public IncomingMessageBase ToIncomingMessage(BotMessage message) => message.Type switch
    {
        MessageType.Group => ToGroupIncomingMessage(message),
        MessageType.Private => ToFriendIncomingMessage(message),
        MessageType.Temp => ToTempIncomingMessage(message),
        _ => throw new NotSupportedException(),
    };

    public FriendIncomingMessage ToFriendIncomingMessage(BotMessage message) => new()
    {
        PeerId = message.Contact.Uin,
        MessageSeq = message.Sequence,
        SenderId = message.Contact.Uin,
        Time = message.Contact.Uin,
        Segments = ToIncomingSegments(message.Entities),
        Friend = Friend((BotFriend)message.Contact),
        ClientSeq = message.ClientSequence,
    };

    public GroupIncomingMessage ToGroupIncomingMessage(BotMessage message) => new()
    {
        PeerId = message.Contact.Uin,
        MessageSeq = message.Sequence,
        SenderId = message.Contact.Uin,
        Time = message.Contact.Uin,
        Segments = ToIncomingSegments(message.Entities),
        Group = Group(((BotGroupMember)message.Contact).Group),
        GroupMember = GroupMember((BotGroupMember)message.Contact)
    };

    public TempIncomingMessage ToTempIncomingMessage(BotMessage message) => new TempIncomingMessage()
    {
        PeerId = message.Contact.Uin,
        MessageSeq = message.Sequence,
        SenderId = message.Contact.Uin,
        Time = message.Contact.Uin,
        Segments = ToIncomingSegments(message.Entities),
    };

    public IReadOnlyList<IIncomingSegment> ToIncomingSegments(MessageChain entities)
    {
        var result = new List<IIncomingSegment>();
        foreach (var entity in entities)
        {
            try
            {
                result.Add(ToIncomingSegment(entity));
            }
            catch (Exception e)
            {
                _logger.LogToIncomingSegmentFailed(entity, e);
            }
        }
        return result;
    }

    public IIncomingSegment ToIncomingSegment(IMessageEntity entity) => entity switch
    {
        // TODO: Need file id
        ImageEntity image => throw new NotImplementedException(),
        MentionEntity mention when mention.Uin != 0 => new MentionSegment
        {
            Data = new MentionData
            {
                UserId = mention.Uin,
            },
        },
        MentionEntity mention when mention.Uin == 0 => new MentionAllSegment { Data = new MentionAllData { } },
        // TODO: Need file id
        RecordEntity record => throw new NotImplementedException(),
        // TODO: Core not implemented
        ReplyEntity reply => throw new NotImplementedException(),
        TextEntity text => new TextSegment { Data = new TextData { Text = text.Text } },
        // TODO: Need file id
        VideoEntity video => throw new NotImplementedException(),
        _ => throw new NotSupportedException(),
    };
}

public static partial class EntityConvertLoggerExtension
{
    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{entity} to incoming segment failed")]
    public static partial void LogToIncomingSegmentFailed(this ILogger<EntityConvert> logger, IMessageEntity entity, Exception e);
}