using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.Events;

public class EventInvoker(BotContext context) : IDisposable
{
    private const string Tag = nameof(EventInvoker);
    
    private readonly Dictionary<Type, Action<BotContext, EventBase>> _actions = new();
    
    private readonly Dictionary<Type, Func<BotContext, EventBase, Task>> _asyncActions = new();

    public delegate void LagrangeEventHandler<in TEvent>(BotContext context, TEvent e) where TEvent : EventBase;
    
    public delegate Task LagrangeAsyncEventHandler<in TEvent>(BotContext context, TEvent e) where TEvent : EventBase;
    
    public void RegisterEvent<TEvent>(LagrangeEventHandler<TEvent> handler) where TEvent : EventBase
    {
        Debug.Assert(!handler.Method.IsStatic);
        ArgumentNullException.ThrowIfNull(handler.Target);
        
        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            Type[] args = [handler.Target.GetType(), typeof(BotContext), typeof(EventBase)];
            var method = new DynamicMethod($"EventInvoker_{typeof(TEvent)}", typeof(void), args, typeof(EventInvoker), true);
            var il = method.GetILGenerator();
            
            il.Emit(OpCodes.Ldarg_0); // [Target]
            il.Emit(OpCodes.Ldarg_1); // [Target, BotContext]
            il.Emit(OpCodes.Ldarg_2); // [Target, BotContext, EventBase]
            il.Emit(OpCodes.Castclass, typeof(TEvent)); // [Target, BotContext, TEvent]
            il.Emit(OpCodes.Callvirt, handler.Method); // []
            il.Emit(OpCodes.Ret); // []

            _actions[typeof(TEvent)] = method.CreateDelegate<Action<BotContext, EventBase>>(handler.Target);
        }
        else
        {
            _actions[typeof(TEvent)] = (ctx, e) => handler(ctx, (TEvent)e);
        }
    }
    
    public void RegisterEvent<TEvent>(LagrangeAsyncEventHandler<TEvent> handler) where TEvent : EventBase
    {
        Debug.Assert(!handler.Method.IsStatic);
        ArgumentNullException.ThrowIfNull(handler.Target);
        
        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            Type[] args = [handler.Target.GetType(), typeof(BotContext), typeof(EventBase)];
            var method = new DynamicMethod($"EventInvoker_{typeof(TEvent)}", typeof(Task), args, typeof(EventInvoker), true);
            var il = method.GetILGenerator();
            
            il.Emit(OpCodes.Ldarg_0); // [Target]
            il.Emit(OpCodes.Ldarg_1); // [Target, BotContext]
            il.Emit(OpCodes.Ldarg_2); // [Target, BotContext, EventBase]
            il.Emit(OpCodes.Castclass, typeof(TEvent)); // [Target, BotContext, TEvent]
            il.Emit(OpCodes.Callvirt, handler.Method); // [Task]
            il.Emit(OpCodes.Ret); // []
            
            _asyncActions[typeof(TEvent)] = method.CreateDelegate<Func<BotContext, EventBase, Task>>(handler.Target);
        }
        else
        {
            _asyncActions[typeof(TEvent)] = (ctx, e) => handler(ctx, (TEvent)e);
        }
    }

    internal void PostEvent<T>(T @event) where T : EventBase => Task.Run(async () =>
    {
        await context.EventContext.HandleOutgoingEvent(@event);
        
        try
        {
            if (_actions.TryGetValue(typeof(T), out var @delegate))
            {
                @delegate(context, @event);
            }
            else if (_asyncActions.TryGetValue(typeof(T), out var asyncDelegate))
            {
                await asyncDelegate(context, @event);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (typeof(T) == typeof(BotLogEvent)) 
            {
                Console.WriteLine($"Failed to post event: {@event}");
                return;
            }

            PostEvent(new BotLogEvent(Tag, LogLevel.Error, $"{ex}"));
        }
    });

    public void Dispose()
    {
        _actions.Clear();
        _asyncActions.Clear();
    }
}