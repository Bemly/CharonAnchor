using Lagrange.Proto.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator.Entity;

public class ProtoFieldInfo(MemberDeclarationSyntax syntax, string name, TypeSyntax typeSyntax, WireType wireType, bool isSigned)
{
    public MemberDeclarationSyntax Syntax { get; } = syntax;
    
    public string Name { get; } = name;
    
    public TypeSyntax TypeSyntax { get; } = typeSyntax;
    
    public WireType WireType { get; } = wireType;
    
    public bool IsSigned { get; } = isSigned;
}
