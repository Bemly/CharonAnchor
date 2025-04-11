using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator.Utility.Extension;

public static class TypeExtension
{
    public static bool IsNumberType(this TypeSyntax type)
    {
        return type switch
        {
            PredefinedTypeSyntax { Keyword.RawKind : >= 8304 and <= 8312 } => true,
            NullableTypeSyntax nullableType => IsNumberType(nullableType.ElementType),
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text switch
            {
                "Boolean" or "Int8" or "Int16" or "Int32" or "Int64" or "UInt8" or "UInt16" or "UInt32" or "UInt64" => true,
                _ => false
            },
            QualifiedNameSyntax qualifiedName => qualifiedName.Right switch
            {
                IdentifierNameSyntax identifierName when qualifiedName.Left.ToString() == "System" => IsNumberType(identifierName),
                _ => false
            },
            _ => false
        };
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
    
    public static bool IsSingleType(this TypeSyntax type)
    {
        return type switch
        {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.IsKind(SyntaxKind.FloatKeyword),
            NullableTypeSyntax nullableType => IsSingleType(nullableType.ElementType),
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text == "Single",
            QualifiedNameSyntax qualifiedName => qualifiedName.Right switch
            {
                IdentifierNameSyntax identifierName when qualifiedName.Left.ToString() == "System" => IsSingleType(identifierName),
                _ => false
            },
            _ => false
        };
    }
    
    public static bool IsDoubleType(this TypeSyntax type)
    {
        return type switch
        {
            PredefinedTypeSyntax predefinedType => predefinedType.Keyword.IsKind(SyntaxKind.DoubleKeyword),
            NullableTypeSyntax nullableType => IsDoubleType(nullableType.ElementType),
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text == "Double",
            QualifiedNameSyntax qualifiedName => qualifiedName.Right switch
            {
                IdentifierNameSyntax identifierName when qualifiedName.Left.ToString() == "System" => IsDoubleType(identifierName),
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