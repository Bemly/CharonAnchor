using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator.Utility.Extension;

public static class RoslynExtension
{
    public static NameSyntax? GetNamespace(this MemberDeclarationSyntax context) => context.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name ?? context.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault()?.Name;

    public static bool ContainsAttribute(this MemberDeclarationSyntax context, string attributeName) => context.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.Name.ToString() == attributeName);

    public static bool IsPartial(this TypeDeclarationSyntax context) => context.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

    public static NameSyntax GetFullName(this Type type) => SyntaxFactory.ParseName($"global::{type.FullName}");
    
    public static ITypeSymbol GetTypeSymbol(this SemanticModel model, TypeSyntax type)
    {
        var symbol = model.GetSymbolInfo(type).Symbol;
        if (symbol is ITypeSymbol typeSymbol)
        {
            return typeSymbol;
        }
        
        throw new InvalidOperationException($"Unable to get type symbol for {type}");
    }
}