using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator.Entity;

public class ProtoFieldInfo(MemberDeclarationSyntax syntax, WireType wireType, bool isSigned)
{
    public MemberDeclarationSyntax Syntax { get; } = syntax;
    
    public WireType WireType { get; } = wireType;
    
    public bool IsSigned { get; } = isSigned;
}
