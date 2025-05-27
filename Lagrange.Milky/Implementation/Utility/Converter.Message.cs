using Lagrange.Core.Common.Entity;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Milky.Implementation.Entity.Message.Incoming;
using Lagrange.Milky.Implementation.Entity.Segment.Common.Data;
using Lagrange.Milky.Implementation.Entity.Segment.Incoming;
using Lagrange.Milky.Implementation.Entity.Segment.Incoming.Data;
using Lagrange.Milky.Implementation.Entity.Segment.Outgoing;
using Microsoft.Extensions.Logging;

namespace Lagrange.Milky.Implementation.Utility;

public partial class Converter
{
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
        Time = new DateTimeOffset(message.Time).ToUnixTimeSeconds(),
        Segments = ToIncomingSegments(message.Entities),
        Friend = Friend((BotFriend)message.Contact),
        ClientSeq = message.ClientSequence,
    };

    public GroupIncomingMessage ToGroupIncomingMessage(BotMessage message)
    {
        var member = (BotGroupMember)message.Contact;
        return new GroupIncomingMessage
        {
            PeerId = member.Group.Uin,
            MessageSeq = message.Sequence,
            SenderId = message.Contact.Uin,
            Time = new DateTimeOffset(message.Time).ToUnixTimeSeconds(),
            Segments = ToIncomingSegments(message.Entities),
            Group = Group(member.Group),
            GroupMember = GroupMember(member)
        };
    }

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
        ImageEntity image => new IncomingImageSegment
        {
            Data = new IncomingImageData
            {
                ResourceId = image.FileUuid,
                Summary = image.Summary,
                SubType = image.SubType switch
                {
                    0 => "normal",
                    1 => "sticker",
                    _ => throw new NotSupportedException(),
                }
            }
        },
        MentionEntity { Uin: not 0 } mention => new IncomingMentionSegment
        {
            Data = new MentionData
            {
                UserId = mention.Uin,
            },
        },
        MentionEntity { Uin: 0 } => new IncomingMentionAllSegment { Data = new MentionAllData { } },
        RecordEntity record => new IncomingRecordSegment
        {
            Data = new IncomingRecordData
            {
                ResourceId = record.FileUuid,
                Duration = (int)record.RecordLength,
            }
        },
        // TODO: Core not implemented
        ReplyEntity => throw new NotImplementedException(),
        TextEntity text => new IncomingTextSegment { Data = new TextData { Text = text.Text } },
        VideoEntity video => new IncomingVideoSegment
        {
            Data = new IncomingVideoData
            {
                ResourceId = video.FileUuid,
            }
        },
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
        // TODO: Core not implemented
        // OutgoingMentionSegment mention => new MentionEntity(mention.Data.UserId, string.Empty),
        OutgoingMentionSegment => throw new NotImplementedException(),
        // TODO: Core not implemented
        // OutgoingMentionAllSegment => new MentionEntity(0, string.Empty),
        OutgoingMentionAllSegment => throw new NotImplementedException(),
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
        OutgoingVideoSegment => throw new NotImplementedException(),
        // TODO: Core not implemented
        // OutgoingVideoSegment video => new VideoEntity(
        //     await UriUtility.ToMemoryStreamAsync(video.Data.Uri, token),
        //     video.Data.ThumbUri != null ? await UriUtility.ToMemoryStreamAsync(video.Data.ThumbUri, token) : null
        // ),
        // TODO:
        // OutgoingForwardSegment => 
        _ => throw new NotSupportedException(),
    };
}

public static partial class EntityConvertLoggerExtension
{
    [LoggerMessage(EventId = 998, Level = LogLevel.Error, Message = "{segment} to message entity failed")]
    public static partial void LogToMessageEntityFailed(this ILogger<Converter> logger, IOutgoingSegment segment, Exception e);

    [LoggerMessage(EventId = 999, Level = LogLevel.Error, Message = "{entity} to incoming segment failed")]
    public static partial void LogToIncomingSegmentFailed(this ILogger<Converter> logger, IMessageEntity entity, Exception e);
}