using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;
using Lagrange.Proto.Utility;

namespace Lagrange.Proto.Nodes;

public sealed partial class ProtoArray : ProtoNode
{
    private readonly List<ProtoNode> _list = [];

    public ProtoArray(WireType wireType, params ReadOnlySpan<ProtoNode> nodes) : base(wireType)
    {
        _list.AddRange(nodes);
        foreach (var node in nodes) node.AssignParent(this);
    }
    
    public ProtoArray(WireType wireType, params ProtoNode[] nodes) : base(wireType)
    {
        _list.AddRange(nodes);
        foreach (var node in nodes) node.AssignParent(this);
    }
    
    public ProtoArray(WireType wireType) : base(wireType) { }

    public override void WriteTo(int field, ProtoWriter writer)
    {
        bool first = true;
        
        foreach (var node in _list)
        {
            if (first) first = false;
            else writer.EncodeVarInt(field << 3 | (int)node.WireType);

            node.WriteTo(field, writer);
        }
    }
    
    public override int Measure(int field)
    {
        int size = ProtoHelper.GetVarIntLength(field << 3 | (int)WireType) * (_list.Count - 1);
        foreach (var node in _list)
        {
            size += node.Measure(field);
        }
        return size;
    }
    
    private protected override ProtoNode GetItem(int index)
    {
        return _list[index];
    }

    private protected override void SetItem(int index, ProtoNode value)
    {
        value.AssignParent(this);
        DetachParent(_list[index]);
        _list[index] = value;
    }
}