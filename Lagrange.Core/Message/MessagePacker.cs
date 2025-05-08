using System.Diagnostics.CodeAnalysis;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Exceptions;
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

    /// <summary>
    /// For the use of MessageService in OneBot
    /// </summary>
    public async Task<BotMessage> Parse(ReadOnlyMemory<byte> src) => await Parse(ProtoHelper.Deserialize<MsgPush>(src.Span));

    public async Task<BotMessage> Parse(MsgPush msgPush)
    {
        var contentHead = msgPush.CommonMessage.ContentHead;
        var routingHead = msgPush.CommonMessage.RoutingHead;
        var elems = msgPush.CommonMessage.MessageBody.RichText.Elems;

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

    public ReadOnlyMemory<byte> Build(BotMessage message)
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
}