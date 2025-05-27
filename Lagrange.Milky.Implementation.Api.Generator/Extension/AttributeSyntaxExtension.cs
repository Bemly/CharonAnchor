using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Milky.Implementation.Api.Generator.Extension;

public static class AttributeSyntaxExtension
{
    public static ClassDeclarationSyntax? GetAnnotatedClass(this AttributeSyntax attribute)
    {
        var current = attribute.Parent;
        while (current != null)
        {
            if (current is ClassDeclarationSyntax @class) return @class;

            current = current.Parent;
        }
        return null;
    }
}