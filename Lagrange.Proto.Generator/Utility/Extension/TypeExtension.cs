using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator.Utility.Extension;

public static class TypeExtension
{
    private static readonly string[] SystemAssemblies = ["mscorlib", "System", "System.Core", "System.Private.CoreLib", "System.Runtime"];
    
    public static bool IsUserDefinedType(this ITypeSymbol type)
    {
        return type.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Enum && !type.ContainingAssembly.IsSystemAssembly();
    }
    
    private static bool IsSystemAssembly(this IAssemblySymbol assembly)
    {
        return SystemAssemblies.Contains(assembly.Name);
    }

    public static bool IsIntegerType(this ITypeSymbol symbol)
    {
        if (symbol.IsNullable())
        {
            return symbol.NullableAnnotation == NullableAnnotation.Annotated && symbol is INamedTypeSymbol { IsGenericType: true, ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } namedTypeSymbol && namedTypeSymbol.TypeArguments[0].IsIntegerType();
        }
        
        return symbol.SpecialType is SpecialType.System_SByte or SpecialType.System_Byte or SpecialType.System_Int16 or
               SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or
               SpecialType.System_Int64 or SpecialType.System_UInt64;
    }
    
    public static bool IsEnumType(this ITypeSymbol symbol)
    {
        return symbol.TypeKind == TypeKind.Enum;
    }
    
    public static bool IsNullable(this ITypeSymbol symbol)
    {
        return symbol is { IsValueType: true, NullableAnnotation: NullableAnnotation.Annotated };
    }
    
    public static bool IsStringType(this TypeSyntax type)
    {
        return type switch
        {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.IsKind(SyntaxKind.StringKeyword),
            NullableTypeSyntax nullableType => IsStringType(nullableType.ElementType),
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text == "String",
            QualifiedNameSyntax qualifiedName => qualifiedName.Right switch
            {
                IdentifierNameSyntax identifierName when qualifiedName.Left.ToString() == "System" => IsStringType(identifierName),
                _ => false
            },
            _ => false
        };
    }
    
    public static bool IsByteArrayType(this TypeSyntax type)
    {
        return type switch
        {
            ArrayTypeSyntax { ElementType: PredefinedTypeSyntax predefinedType } =>  predefinedType.Keyword.IsKind(SyntaxKind.ByteKeyword),
            ArrayTypeSyntax { ElementType: NullableTypeSyntax nullableType } => IsByteArrayType(nullableType.ElementType),
            ArrayTypeSyntax { ElementType: IdentifierNameSyntax identifierName } => identifierName.Identifier.Text == "Byte",
            ArrayTypeSyntax { ElementType: QualifiedNameSyntax qualifiedName } => qualifiedName.Right switch
            {
                IdentifierNameSyntax identifierName when qualifiedName.Left.ToString() == "System" => IsByteArrayType(identifierName),
                _ => false
            },
            _ => false
        };
    }
    
    public static bool IsNullableType(this TypeSyntax type)
    {
        return type switch
        {
            NullableTypeSyntax => true,
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text == "Nullable",
            QualifiedNameSyntax qualifiedName => qualifiedName.Right switch
            {
                IdentifierNameSyntax identifierName when qualifiedName.Left.ToString() == "System" => IsNullableType(identifierName),
                _ => false
            },
            _ => false
        };
    }
}