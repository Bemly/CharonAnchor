using Lagrange.Core.Common;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Implementation.Entity;
using Lagrange.Milky.Implementation.Entity.Message.Incoming;
using Lagrange.Milky.Implementation.Entity.Segment.Common.Data;
using Lagrange.Milky.Implementation.Entity.Segment.Incoming;
using Lagrange.Milky.Implementation.Entity.Segment.Outgoing;
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

    public TempIncomingMessage ToTempIncomingMessage(BotMessage message) => new()
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
        ImageEntity => throw new NotImplementedException(),
        MentionEntity { Uin: not 0 } mention => new IncomingMentionSegment
        {
            Data = new MentionData
            {
                UserId = mention.Uin,
            },
        },
        MentionEntity { Uin: 0 } => new IncomingMentionAllSegment { Data = new MentionAllData { } },
        // TODO: Need file id
        RecordEntity => throw new NotImplementedException(),
        // TODO: Core not implemented
        ReplyEntity => throw new NotImplementedException(),
        TextEntity text => new IncomingTextSegment { Data = new TextData { Text = text.Text } },
        // TODO: Need file id
        VideoEntity => throw new NotImplementedException(),
        _ => throw new NotSupportedException(),
    };

    public async Task<MessageChain> ToMessageChainAsync(IReadOnlyList<IOutgoingSegment> segments, CancellationToken token)
    {
        var chain = new MessageChain();
        foreach (var segment in segments)
        {
            chain.Add(await ToMessageEntityAsync(segment, token));
        }
        return chain;
    }

    public async Task<IMessageEntity> ToMessageEntityAsync(IOutgoingSegment segment, CancellationToken token) => segment switch
    {
        OutgoingTextSegment text => new TextEntity(text.Data.Text),
        OutgoingMentionSegment mention => new MentionEntity(mention.Data.UserId, string.Empty),
        OutgoingMentionAllSegment => new MentionEntity(0, string.Empty),
        // TODO: Core not implemented
        OutgoingFaceSegment => throw new NotImplementedException(),
        // TODO: Core not implemented
        OutgoingReplySegment => throw new NotImplementedException(),
        OutgoingImageSegment image => new ImageEntity(
            await UriUtility.ToMemoryStreamAsync(image.Data.Uri, token),
            image.Data.Summary,
            image.Data.SubType switch
            {
                "normal" => 0,
                "sticker" => 1,
                _ => throw new NotSupportedException(),
            }
        ),
        OutgoingRecordSegment record => new RecordEntity(await UriUtility.ToMemoryStreamAsync(record.Data.Uri, token)),
        OutgoingVideoSegment video => new VideoEntity(
            await UriUtility.ToMemoryStreamAsync(video.Data.Uri, token),
            video.Data.ThumbUri != null ? await UriUtility.ToMemoryStreamAsync(video.Data.ThumbUri, token) : null
        ),
        // TODO:
        // OutgoingForwardSegment => 
        _ => throw new NotSupportedException(),
    };
}

public static partial class EntityConvertLoggerExtension
{
    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{segment} to message entity failed")]
    public static partial void LogToMessageEntityFailed(this ILogger<EntityConvert> logger, IOutgoingSegment segment, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{entity} to incoming segment failed")]
    public static partial void LogToIncomingSegmentFailed(this ILogger<EntityConvert> logger, IMessageEntity entity, Exception e);
}