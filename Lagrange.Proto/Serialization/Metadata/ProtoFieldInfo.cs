using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Lagrange.Proto.Primitives;

namespace Lagrange.Proto.Serialization.Metadata;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class ProtoFieldInfo(int field, WireType wireType, Type declared, Type property)
{
    public int Field { get; } = field;
    
    public WireType WireType { get; } = wireType;
    
    public Type DeclaredType { get; } = declared;
    
    public Type PropertyType { get; } = property;
    
    public ProtoNumberHandling NumberHandling { get; init; } = ProtoNumberHandling.Default;
    
    internal ProtoConverter EffectiveConverter
    {
        get
        {
            Debug.Assert(_effectiveConverter != null);
            return _effectiveConverter;
        }
    }
    
    public Func<object, object?>? Get { get => _untypedGet; set => SetGetter(value); }
    public Action<object, object?>? Set { get => _untypedSet; set => SetSetter(value); }    
    
    private protected Func<object, object?>? _untypedGet;
    private protected Action<object, object?>? _untypedSet;
    
    private protected ProtoConverter? _effectiveConverter;
    private protected abstract void SetGetter(Delegate? getter);
    private protected abstract void SetSetter(Delegate? setter);

    public abstract void Read(WireType wireType, ref ProtoReader reader, object target);
        
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"Field = {Field}, WireType = {WireType}, PropertyType = {PropertyType}, DeclaredType = {DeclaredType}";
    
}

public class ProtoFieldInfo<T> : ProtoFieldInfo
{
    public ProtoFieldInfo(int field, WireType wireType, Type declared) : base(field, wireType, declared, typeof(T))
    {
        var converter = ProtoTypeResolver.GetConverter<T>();
        _effectiveConverter = converter;
        _typedEffectiveConverter = converter;
    }
    
    private Func<object, T>? _typedGet;
    private Action<object, T>? _typedSet;

    public new Func<object, T>? Get
    {
        get => _typedGet;
        set => SetGetter(value);
    }

    public new Action<object, T>? Set
    {
        get => _typedSet;
        set => SetSetter(value);
    }
    
    internal new ProtoConverter<T> EffectiveConverter
    {
        get
        {
            Debug.Assert(_typedEffectiveConverter != null);
            return _typedEffectiveConverter;
        }
    }

    private ProtoConverter<T>? _typedEffectiveConverter;

    private protected override void SetGetter(Delegate? getter)
    {
        Debug.Assert(getter is null or Func<object, object?> or Func<object, T>);

        switch (getter)
        {
            case null:
                _typedGet = null;
                _untypedGet = null;
                break;
            case Func<object, T> typedGetter:
                _typedGet = typedGetter;
                _untypedGet = getter as Func<object, object?> ?? (obj => typedGetter(obj));
                break;
            default:
                var untypedGet = (Func<object, object?>)getter;
                _typedGet = obj => (T)untypedGet(obj)!;
                _untypedGet = untypedGet;
                break;
        }
    }

    private protected override void SetSetter(Delegate? setter)
    {
        Debug.Assert(setter is null or Action<object, object?> or Action<object, T>);

        switch (setter)
        {
            case null:
                _typedSet = null;
                _untypedSet = null;
                break;
            case Action<object, T> typedSetter:
                _typedSet = typedSetter;
                _untypedSet = setter as Action<object, object?> ?? ((obj, value) => typedSetter(obj, (T)value!));
                break;
            default:
                var untypedSet = (Action<object, object?>)setter;
                _typedSet = (obj, value) => untypedSet(obj, value);
                _untypedSet = untypedSet;
                break;
        }
    }
    
    public override void Read(WireType wireType, ref ProtoReader reader, object target)
    {
        Debug.Assert(_typedEffectiveConverter != null);
        
        T value = _typedEffectiveConverter.Read(Field, wireType, ref reader);
        _typedSet?.Invoke(target, value);
    }
}