using System.Diagnostics;

namespace Lagrange.Proto.Serialization.Metadata;

public abstract class ProtoFieldInfo(int field, WireType wireType, Type declared, Type property)
{
    public int Field { get; } = field;
    
    public WireType WireType { get; } = wireType;
    
    public Type DeclaredType { get; } = declared;
    
    public Type PropertyType { get; } = property;
    
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
}

public class ProtoFieldInfo<T>(int field, WireType wireType, Type declared) : ProtoFieldInfo(field, wireType, declared, typeof(T))
{
    private Func<object, T>? _typedGet;
    private Action<object, T>? _typedSet;

    internal new Func<object, T>? Get
    {
        get => _typedGet;
        set => SetGetter(value);
    }

    internal new Action<object, T>? Set
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
}