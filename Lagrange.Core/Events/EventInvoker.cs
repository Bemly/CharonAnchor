using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Lagrange.Core.Events.EventArgs;

namespace Lagrange.Core.Events;

public sealed class EventInvoker(BotContext context) : IDisposable
{
    private const string Tag = nameof(EventInvoker);

    private readonly ConcurrentDictionary<Type, Delegate> _syncHandlers = new();
    private readonly ConcurrentDictionary<Type, Delegate> _asyncHandlers = new();

    public delegate void  LagrangeEventHandler <in TEvent>(BotContext ctx, TEvent e) where TEvent : EventBase;
    public delegate Task  LagrangeAsyncEventHandler<in TEvent>(BotContext ctx, TEvent e) where TEvent : EventBase;

    public void RegisterEvent<TEvent>(LagrangeEventHandler<TEvent> handler) where TEvent : EventBase
    {
        Debug.Assert(!handler.Method.IsStatic);
        
        AddHandler(_syncHandlers, typeof(TEvent), BuildSyncDelegate(handler));
    }

    public void RegisterEvent<TEvent>(LagrangeAsyncEventHandler<TEvent> handler) where TEvent : EventBase
    {
        Debug.Assert(!handler.Method.IsStatic);
        
        AddHandler(_asyncHandlers, typeof(TEvent), BuildAsyncDelegate(handler));
    }

    public void UnregisterEvent<TEvent>(LagrangeEventHandler<TEvent> handler) where TEvent : EventBase => RemoveHandler(_syncHandlers, typeof(TEvent), handler);

    public void UnregisterEvent<TEvent>(LagrangeAsyncEventHandler<TEvent> handler) where TEvent : EventBase => RemoveHandler(_asyncHandlers, typeof(TEvent), handler);
    
    public void UnregisterEvent<TEvent>() where TEvent : EventBase
    {
        _syncHandlers.TryRemove(typeof(TEvent), out _);
        _asyncHandlers.TryRemove(typeof(TEvent), out _);
    }

    internal void PostEvent<T>(T ev) where T : EventBase => Task.Run(async () =>
    {
        await context.EventContext.HandleOutgoingEvent(ev);

        try
        {
            if (_syncHandlers.TryGetValue(typeof(T), out var @delegate))
            {
                ((Action<BotContext, EventBase>)@delegate).Invoke(context, ev);
            }

            if (_asyncHandlers.TryGetValue(typeof(T), out var asyncDelegate))
            {
                await ((Func<BotContext, EventBase, Task>)asyncDelegate)(context, ev);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (typeof(T) == typeof(BotLogEvent))
            {
                Console.WriteLine($"Failed to post event: {ev}");
                return;
            }
            PostEvent(new BotLogEvent(Tag, LogLevel.Error, ex.ToString(), ex));
        }
    });

    public void Dispose()
    {
        _syncHandlers.Clear();
        _asyncHandlers.Clear();
    }

    private static void AddHandler(ConcurrentDictionary<Type, Delegate> dict, Type key, Delegate handler)
    {
        dict.AddOrUpdate(key, handler, (_, old) => Delegate.Combine(old, handler));
    }

    private static void RemoveHandler(ConcurrentDictionary<Type, Delegate> dict, Type key, Delegate handler)
    {
        dict.AddOrUpdate(key, _ => null!, (_, old) =>
        {
            var updated = Delegate.Remove(old, handler);
            return updated != null && updated.GetInvocationList().Length > 0 ? updated : null!;
        });
        
        if (dict.TryRemove(key, out var d) && d is not { }) dict.TryRemove(key, out _); // is not { } is needed to avoid removing the key if the delegate is null, and escape from ReSharper's warning
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "DynamicMethod is not supported in AOT, but this branch is skipped there")]
    private static Action<BotContext, EventBase> BuildSyncDelegate<TEvent>(LagrangeEventHandler<TEvent> h) where TEvent : EventBase
    {
        if (!RuntimeFeature.IsDynamicCodeSupported) return (ctx, ev) => h(ctx, (TEvent)ev);

        var dm = new DynamicMethod($"EventInvoker_{typeof(TEvent).Name}", typeof(void), [h.Target!.GetType(), typeof(BotContext), typeof(EventBase)], typeof(EventInvoker), true);
        var il = dm.GetILGenerator();
        
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Castclass, typeof(TEvent));
        il.Emit(OpCodes.Callvirt, h.Method);
        il.Emit(OpCodes.Ret);

        return dm.CreateDelegate<Action<BotContext, EventBase>>(h.Target);
    }

    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "DynamicMethod is not supported in AOT, but this branch is skipped there")]
    private static Func<BotContext, EventBase, Task> BuildAsyncDelegate<TEvent>(LagrangeAsyncEventHandler<TEvent> h) where TEvent : EventBase
    {
        if (!RuntimeFeature.IsDynamicCodeSupported) return (ctx, ev) => h(ctx, (TEvent)ev);

        var dm = new DynamicMethod($"EventInvoker_{typeof(TEvent).Name}_Async", typeof(Task), [h.Target!.GetType(), typeof(BotContext), typeof(EventBase)], typeof(EventInvoker), true);
        var il = dm.GetILGenerator();
        
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Castclass, typeof(TEvent));
        il.Emit(OpCodes.Callvirt, h.Method);
        il.Emit(OpCodes.Ret);

        return dm.CreateDelegate<Func<BotContext, EventBase, Task>>(h.Target);
    }
}
