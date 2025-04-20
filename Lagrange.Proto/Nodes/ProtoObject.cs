using Lagrange.Proto.Primitives;
using Lagrange.Proto.Serialization;

namespace Lagrange.Proto.Nodes;

public partial class ProtoObject() : ProtoNode(WireType.LengthDelimited)
{
    public override void WriteTo(int field, ProtoWriter writer)
    {
        foreach (var (f, node) in _fields)
        {
            writer.EncodeVarInt(f << 3 | (int)node.WireType);
            node.WriteTo(f, writer);
        }
    }

    private protected override ProtoNode GetItem(int field)
    {
        if (_fields.TryGetValue(field, out var node)) return node;

        throw new KeyNotFoundException($"Field {field} not found in ProtoObject.");
    }

    private protected override void SetItem(int field, ProtoNode node)
    {
        if (_fields.TryGetValue(field, out var removed))
        {
            DetachParent(removed);
            var array = new ProtoArray(removed.WireType, removed, node);
            node = array;
        }
        
        _fields[field] = node;
        node.AssignParent(this);
    }
}