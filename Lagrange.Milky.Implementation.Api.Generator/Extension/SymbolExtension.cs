using Microsoft.CodeAnalysis;

namespace Lagrange.Milky.Implementation.Api.Generator.Extension;

public static class SymbolExtension
{
    public static bool SEquals(this ISymbol? left, ISymbol? right)
    {
        return SymbolEqualityComparer.Default.Equals(left, right);
    }
}