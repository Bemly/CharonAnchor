using System.Diagnostics.CodeAnalysis;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
using Lagrange.Core.Internal.Events.System;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message.Entities;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Extension;


namespace Lagrange.Core.Message;

internal class MessagePacker
{
    private readonly BotContext _context;
    
    private readonly List<IMessageEntity> _factory;
    
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public MessagePacker(BotContext context)
    {
        _context = context;
        _factory = [];

        foreach (var type in typeof(MessagePacker).Assembly.GetTypes())
        {
            if (type.HasImplemented<IMessageEntity>())
            {
                _factory.Add((IMessageEntity?)Activator.CreateInstance(type) ?? throw new InvalidOperationException());
            }
        }
    }

    public async Task<BotMessage> Parse(CommonMessage msg)
    {
        var contentHead = msg.ContentHead;
        var routingHead = msg.RoutingHead;
        var elems = msg.MessageBody.RichText.Elems;

        var contact = await ResolveContact(contentHead.Type, routingHead);
        var message = new BotMessage(contact, (contact as BotGroupMember)?.Group)
        {
            MessageId = contentHead.MsgUid, // MsgUid & 0xFFFFFFFF are the same to random
            Time = DateTimeOffset.FromUnixTimeSeconds(contentHead.Time).DateTime,
            Sequence = contentHead.Sequence,
            ClientSequence = contentHead.ClientSequence,
            Random = contentHead.Random
        };
        
        foreach (var elem in elems)
        {
            foreach (var factory in _factory)
            {
                if (factory.Parse(elems, elem) is not { } entity) continue;

                message.Entities.Add(entity);
                break;
            }
        }

        foreach (var entity in message.Entities)
        {
            await entity.Postprocess(_context, message);
        }

        return message;
    }

    private async Task<BotContact> ResolveContact(int type, RoutingHead routingHead)
    {
        switch (type)
        {
            case 166:
                var friend = await _context.CacheContext.ResolveFriend(routingHead.FromUin);
                if (friend == null) throw new InvalidTargetException(routingHead.FromUin);

                return friend;
            case 141:
                return new BotStranger(routingHead.FromUin, "", routingHead.FromUid)
                {
                    Source = routingHead.CommonC2C.FromTinyId
                };
            case 82:
                var items = await _context.CacheContext.ResolveMember(routingHead.Group.GroupCode, routingHead.FromUin);
                if (items == null) throw new InvalidTargetException(routingHead.Group.GroupCode, routingHead.FromUin);

                return items.Value.Item2;
            default:
                throw new NotImplementedException();
        }
    }

    public static ReadOnlyMemory<byte> Build(BotMessage message)
    {
        var routingHead = new SendRoutingHead();

        switch (message.Contact)
        {
            case BotFriend friend:
                routingHead.C2C = new C2C { PeerUin = friend.Uin, PeerUid = friend.Uid };
                break;
            case BotStranger:
                throw new InvalidOperationException();
        }
        
        if (message.Group != null)
        {
            routingHead.Group = new Grp { GroupUin = message.Group.GroupUin };
        }

        var messageBody = new MessageBody();
        foreach (var entity in message.Entities)
        {
            if (entity.Build() is not { } elem) continue;
            messageBody.RichText.Elems.AddRange(elem);
        }

        var proto = new PbSendMsgReq
        {
            RoutingHead = routingHead,
            ContentHead = new SendContentHead
            {
                PkgNum = 1,
                PkgIndex = 0,
                DivSeq = 0,
                AutoReply = 0
            },
            MessageBody = messageBody,
            ClientSequence = message.ClientSequence,
            Random = message.Random,
        };
        return ProtoHelper.Serialize(proto);
    }

    public static ReadOnlyMemory<byte> BuildTrans0X211(BotFriend friend, FileUploadEventReq req, FileUploadEventResp resp, int clientSequence, uint random)
    {
        var extra = new FileExtra
        {
            File = new NotOnlineFile
            {
                FileType = 0,
                FileUuid = resp.FileId,
                FileMd5 = req.FileMd5,
                FileName = req.FileName,
                FileSize = (ulong)req.FileStream.Length,
                SubCmd = 1,
                DangerLevel = 0,
                ExpireTime = (uint)DateTimeOffset.Now.AddDays(7).ToUnixTimeSeconds(),
                FileIdCrcMedia = resp.CrcMedia
            }
        };
            
        var proto = new PbSendMsgReq
        {
            RoutingHead = new SendRoutingHead
            {
                Trans0X211 = new Trans0X211
                {
                    ToUin = friend.Uin,
                    CcCmd = 4,
                    Uid = friend.Uid
                }
            },
            ContentHead = new SendContentHead
            {
                PkgNum = 1,
                PkgIndex = 0,
                DivSeq = 0,
                AutoReply = 0
            },
            MessageBody = new MessageBody
            {
                MsgContent = ProtoHelper.Serialize(extra)
            },
            ClientSequence = clientSequence,
            Random = random
        };
        return ProtoHelper.Serialize(proto);
    }

    public static Task<CommonMessage> BuildFake(BotMessage msg)
    {
        var proto = new CommonMessage
        {
            RoutingHead = msg.Contact switch
            {
                BotGroupMember member => new RoutingHead
                {
                    Group = new CommonGroup
                    {
                        GroupCode = member.Group.GroupUin,
                        GroupCard = member.MemberCard ?? member.Uin.ToString(),
                        GroupCardType = 2
                    }
                },
                BotFriend friend => new RoutingHead { CommonC2C = new CommonC2C { Name = friend.Nickname } },
                BotStranger stranger => new RoutingHead { CommonC2C = new CommonC2C { Name = stranger.Nickname } },
                _ => throw new ArgumentOutOfRangeException(nameof(msg.Contact))
            },
            ContentHead = new ContentHead
            {
                Type = msg.Contact switch
                {
                    BotGroupMember => 82,
                    BotFriend => 166,
                    BotStranger => 141,
                    _ => throw new ArgumentOutOfRangeException(nameof(msg.Contact))
                },
                Random = msg.Random,
                Sequence = msg.Sequence,
                Time = new DateTimeOffset(msg.Time).ToUnixTimeSeconds(),
                ClientSequence = msg.ClientSequence,
                MsgUid = msg.MessageId,
            },
            MessageBody = new MessageBody { RichText = new RichText { Elems = [] } }
        };
        
        proto.RoutingHead.FromUin = msg.Contact.Uin;
        proto.RoutingHead.FromUid = msg.Contact.Uid;

        foreach (var entity in msg.Entities)
        {
            if (entity.Build() is not { } elem) continue;
            proto.MessageBody.RichText.Elems.AddRange(elem);
        }

        return Task.FromResult(proto);
    }
}