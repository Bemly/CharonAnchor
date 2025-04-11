using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator.Utility.Extension;

public static class SyntaxExtension
{
    public static StatementSyntax AddBlankLine(this StatementSyntax syntax)
    {
        return syntax.WithTrailingTrivia(syntax.GetTrailingTrivia().Add(SyntaxFactory.Comment("\n")));
    }
}