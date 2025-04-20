using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Nodes;

public abstract partial class ProtoNode(WireType wireType)
{
    public ProtoNode? Parent { get; internal set; }
    
    public WireType WireType { get; } = wireType;

    internal void AssignParent(ProtoNode parent)
    {
        if (Parent != null) ThrowHelper.ThrowInvalidOperationException_NodeAlreadyHasParent();

        var p = parent;
        while (p != null)
        {
            if (p == this) ThrowHelper.ThrowInvalidOperationException_NodeCycleDetected();
            p = p.Parent;
        }

        Parent = parent;
    }

    public ProtoArray AsArray()
    {
        if (this is ProtoArray array) return array;
        
        return new ProtoArray(WireType, this);
    }

    public ProtoObject AsObject()
    {
        if (this is ProtoObject obj) return obj;

        ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(ProtoObject));
        return null;
    }
    
    public ProtoValue AsValue()
    {
        if (this is ProtoValue value) return value;

        ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(ProtoValue));
        return null;
    }
    
    public ProtoNode Root
    {
        get
        {
            var parent = Parent;
            if (parent == null) return this;
            while (parent.Parent != null) parent = parent.Parent;

            return parent;
        }
    }
    
    public ProtoNode this[int field]
    {
        get => GetItem(field);
        set => SetItem(field, value);
    }

    private protected virtual ProtoNode GetItem(int field)
    {
        ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(ProtoArray), nameof(ProtoObject));
        return null;
    }

    private protected virtual void SetItem(int field, ProtoNode node) =>
        ThrowHelper.ThrowInvalidOperationException_NodeWrongType(nameof(ProtoArray), nameof(ProtoObject));
}