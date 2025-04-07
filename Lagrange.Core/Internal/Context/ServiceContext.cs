using System.Collections.Frozen;
using System.Reflection;
using Lagrange.Core.Common;
using Lagrange.Core.Internal.Events;
using Lagrange.Core.Internal.Packets.Struct;
using Lagrange.Core.Internal.Services;

namespace Lagrange.Core.Internal.Context;

internal class ServiceContext
{
    private const string Tag = nameof(ServiceContext);
    
    private int _sequence = Random.Shared.Next(5000000, 9900000);
    
    private readonly FrozenDictionary<string, IService> _services;
    private readonly FrozenDictionary<Type, (ServiceAttribute Attribute, IService Instance)> _servicesEventType;

    private readonly BotContext _context;
    
    public ServiceContext(BotContext context)
    {
        _context = context;
        
        var services = new Dictionary<string, IService>();
        var servicesEventType = new Dictionary<Type, (ServiceAttribute, IService)>();

        foreach (var type in typeof(IService).Assembly.GetTypes()) 
        {
            if (type.GetCustomAttribute<ServiceAttribute>() is { } attr && type.GetInterfaces().Contains(typeof(IService)) && !type.IsAbstract)
            {
                var service = (IService?)Activator.CreateInstance(type) ?? throw new InvalidOperationException("Failed to create service instance");
                services[attr.Command] = service;
                
                foreach (var attribute in type.GetCustomAttributes<EventSubscribeAttribute>())
                {
                    if ((attribute.Protocol & context.Config.Protocol) == Protocols.None) return; // skip if not supported
                    
                    servicesEventType[attribute.EventType] = !servicesEventType.ContainsKey(attribute.EventType)
                        ? (attr, service)
                        : throw new InvalidOperationException($"Multiple services for event type: {attribute.EventType}");
                }
            }
        }
        
        _services = services.ToFrozenDictionary();
        _servicesEventType = servicesEventType.ToFrozenDictionary();
    }

    public ValueTask<ProtocolEvent?> Resolve(SsoPacket ssoPacket)
    {
        if (!_services.TryGetValue(ssoPacket.Command, out var service))
        {
            _context.LogWarning(Tag, $"Service not found for command: {ssoPacket.Command}");
            return new ValueTask<ProtocolEvent?>(default(ProtocolEvent));
        }
        
        _context.LogDebug(Tag, $"Incoming SSOFrame: {ssoPacket.Command}");
        return service.Parse(ssoPacket.Data, _context);
    }

    public async ValueTask<(SsoPacket, ServiceAttribute)> Resolve(ProtocolEvent @event)
    {
        if (!_servicesEventType.TryGetValue(@event.GetType(), out var handler)) return default;
        
        var (attr, service) = handler;
        _context.LogDebug(Tag, $"Outgoing SSOFrame: {handler.Attribute.Command}");
        
        return (new SsoPacket(attr.Command, await service.Build(@event, _context), GetNewSequence()), attr);
    }

    private int GetNewSequence()
    {
        Interlocked.CompareExchange(ref _sequence, 5000000, 9900000);
        return Interlocked.Increment(ref _sequence);
    }
}