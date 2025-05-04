using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Lagrange.Core;
using Lagrange.Core.Common.Entity;
using Lagrange.Core.Events.EventArgs;
using Lagrange.Core.Internal.Packets.Message;
using Lagrange.Core.Message;
using Lagrange.Core.Utility;
using Lagrange.Core.Utility.Extension;
using Lagrange.OneBot.Database;
using Lagrange.OneBot.Entity.Action;
using Lagrange.OneBot.Entity.Message;
using Lagrange.OneBot.Network;
using Lagrange.OneBot.Utility.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JsonHelper = Lagrange.OneBot.Utility.JsonHelper;

namespace Lagrange.OneBot.Message;

public partial class MessageService
{
    private readonly ILogger<MessageService> _logger;
    private readonly MessageOption _option = new();

    private readonly FrozenDictionary<Type, List<(string Type, string SendType, Type Factory)>> _entityToFactory;
    private readonly FrozenDictionary<string, (Type Factory, Type Entity)> _segmentToFactory;

    private readonly ServiceProvider _service;

    private readonly BotContext _context;
    
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "All the types are preserved in the csproj by using the TrimmerRootAssembly attribute")]
    public MessageService(ILogger<MessageService> logger, IConfiguration config, BotContext context, StorageService storage, LagrangeWebSvcProxy proxy)
    {
        _logger = logger;
        config.GetSection("Message").Bind(_option);
        
        _context = context;
        context.EventInvoker.RegisterEvent<BotMessageEvent>(async (bot, @event) =>
        {
            object oneBotMsg = ConvertToOneBotMessage(@event.Message);
            _ = proxy.SendJsonAsync(oneBotMsg);
            await storage.SaveMessage(@event.Message, @event.RawMessage.ToArray());
        });
        
        var service = new ServiceCollection();

        var entityToFactory = new Dictionary<Type, List<(string Type, string SendType, Type Factory)>>();
        var segmentToFactory = new Dictionary<string, (Type, Type)>();
        foreach (var type in typeof(MessageService).Assembly.GetTypes())
        {
            if (!type.HasImplemented<ISegmentFactory>()) continue;
            service.AddSingleton(type);
            
            var attributes = type.GetCustomAttributes<SegmentSubscriberAttribute>();
            foreach (var attribute in attributes)
            {
                if (!entityToFactory.TryGetValue(attribute.Entity, out var factories))
                {
                    factories = [(attribute.SendType, attribute.SendType, type)];
                    entityToFactory[attribute.Entity] = factories;
                }
                else
                {
                    factories.Add((attribute.Type, attribute.SendType, type));
                }
                
                segmentToFactory[attribute.SendType] = (type, attribute.Entity);
            }
        }
        _entityToFactory = entityToFactory.ToFrozenDictionary();
        _segmentToFactory = segmentToFactory.ToFrozenDictionary();

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

    public MessageChain ConvertToChain(OneBotMessage message)
    {
        var builder = new MessageBuilder();
        
        foreach (var segment in message.Messages)
        {
            if (_segmentToFactory.TryGetValue(segment.Type, out var instance))
            {
                var factory = (ISegmentFactory)_service.GetRequiredService(instance.Factory);

                if (JsonHelper.Deserialize((JsonElement)segment.Data, factory.SegmentType) is ISegment data)
                {
                    factory.Build(builder, data);
                }
                else
                {
                    Log.LogCQFailed(_logger, segment.Type);
                }
            }
        }

        return builder.Build();
    }
    
    private List<OneBotSegment> ConvertSegments(BotMessage chain)
    {
        var result = new List<OneBotSegment>();

        foreach (var entity in chain.Entities)
        {
            var factories = _entityToFactory[entity.GetType()];
            foreach (var (_, sendType, factory) in factories)
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
    public ReadOnlyMemory<byte> ConvertSendMsgToPush(BotMessage message)
    {
        Debug.Assert(message.Contact is BotFriend or BotStranger);
        
        var messageBody = new MessageBody();
        foreach (var entity in message.Entities)
        {
            if (entity.Build() is not { } elem) continue;
            messageBody.RichText.Elems.AddRange(elem);
        }
        
        var push = new MsgPush
        {
            CommonMessage = new CommonMessage
            {
                RoutingHead = new RoutingHead
                {
                    FromUin = _context.BotUin,
                    FromUid = _context.Keystore.Uid,
                    ToUin = message.Contact.Uin,
                    ToUid = message.Contact.Uid,
                },
                ContentHead = new ContentHead
                {
                    Random = message.Random,
                    Sequence = message.Sequence,
                    ClientSequence = message.ClientSequence,
                    MsgUid = 0x10000000ul << 32 | message.Random,
                },
                MessageBody = messageBody
            }
        };

        return ProtoHelper.Serialize(push);
    }
    
    private static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = Microsoft.Extensions.Logging.LogLevel.Warning, Message = "Segment {type} Deserialization failed")]
        public static partial void LogCQFailed(ILogger logger, string type);
    }
}