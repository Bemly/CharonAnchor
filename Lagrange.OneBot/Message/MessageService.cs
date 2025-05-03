using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Utility.Extension;
using Lagrange.OneBot.Database;
using Lagrange.OneBot.Entity.Message;
using Lagrange.OneBot.Network;
using Lagrange.OneBot.Utility.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lagrange.OneBot.Message;

public class MessageService
{
    private readonly MessageOption _option = new();

    private readonly Dictionary<Type, List<(string SendType, Type Factory)>> _entityToFactory = new();

    private readonly ServiceProvider _service;

    private readonly BotContext _context;
    
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public MessageService(IConfiguration config, BotContext context, StorageService storage, LagrangeWebSvcProxy proxy)
    {
        config.GetSection("Message").Bind(_option);
        
        _context = context;
        context.EventInvoker.RegisterEvent<BotMessageEvent>(async (bot, @event) =>
        {
            object oneBotMsg = ConvertToOneBotMessage(@event.Message);
            _ = proxy.SendJsonAsync(oneBotMsg);
            await storage.SaveMessage(@event);
        });
        
        var service = new ServiceCollection();

        foreach (var type in typeof(MessageService).Assembly.GetTypes())
        {
            if (!type.HasImplemented<ISegmentFactory>()) continue;
            
            service.AddSingleton(type);
            
            var attributes = type.GetCustomAttributes<SegmentSubscriberAttribute>();
            foreach (var attribute in attributes)
            {
                if (!_entityToFactory.TryGetValue(attribute.Entity, out var factories))
                {
                    factories = [(attribute.SendType, type)];
                    _entityToFactory[attribute.Entity] = factories;
                }
                else
                {
                    factories.Add((attribute.SendType, type));
                }
            }
        }

        service.AddSingleton(storage);
        service.AddLogging();
        _service = service.BuildServiceProvider();
    }

    private object ConvertToOneBotMessage(BotMessage message)
    {
        long self = _context.BotUin;
        long time = new DateTimeOffset(message.Time).ToUnixTimeMilliseconds();
        int messageId = StorageService.CalcMessageHash(message.MessageId, message.Sequence);
        var segments = ConvertSegments(message);
        
        switch (message.Contact)
        {
            case BotStranger stranger:
            {
                var sender = new OneBotSender(stranger.Uin, stranger.Nickname);

                return new OneBotPrivateMessage(self, sender, "group", time, segments)
                {
                    UserId = stranger.Uin,
                    MessageId = messageId,
                    TargetId = self
                };
            }
            case BotFriend friend:
            {
                var sender = new OneBotSender(friend.Uin, friend.Nickname)
                {
                    Age = friend.Age,
                    Sex = friend.Gender.ToOneBotString()
                };
                
                return new OneBotPrivateMessage(self, sender, "friend", time, segments)
                {
                    UserId = friend.Uin,
                    MessageId = messageId,
                    TargetId = self
                };
            }
            case BotGroupMember member when message.Group is { } group:
            {
                return new OneBotGroupMessage(self, group.GroupUin, segments, "", member, messageId, time);
            }
            default:
            {
                throw new InvalidOperationException();
            }
        }
    }
    
    private List<OneBotSegment> ConvertSegments(BotMessage chain)
    {
        var result = new List<OneBotSegment>();

        foreach (var entity in chain.Entities)
        {
            var factories = _entityToFactory[entity.GetType()];
            foreach (var (sendType, factory) in factories)
            {
                var instance = (ISegmentFactory)_service.GetRequiredService(factory);
                if (instance.Parse(chain, entity) is { } segment)
                {
                    result.Add(new OneBotSegment(sendType, segment));
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Only for the use of C2C send Message
    /// </summary>
    private MsgPush ConvertSendMsgToPush(PbSendMsgReq send) // TODO: Response
    {
        return new MsgPush
        {
            CommonMessage = new CommonMessage
            {
                RoutingHead = new RoutingHead
                {
                    FromUin = _context.BotUin,
                    FromUid = _context.Keystore.Uid,
                    ToUin = send.RoutingHead.C2C.PeerUin,
                    ToUid = send.RoutingHead.C2C.PeerUid
                },
                ContentHead = new ContentHead
                {
                    Random = send.Random,
                    ClientSequence = send.ClientSequence,
                    MsgUid = 0x10000000ul << 32 | send.Random,
                },
                MessageBody = send.MessageBody
            }
        };
    }
}