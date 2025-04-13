using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SK = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Proto.Generator
{
    public partial class ProtoSourceGenerator
    {
        private partial class Emitter
        {
            private readonly string _protoTypeInfoFullName = $"global::Lagrange.Proto.Serialization.Metadata.ProtoObjectInfo<{parser.Identifier}>";
            
            private readonly string _protoTypeInfoNullableFullName = $"global::Lagrange.Proto.Serialization.Metadata.ProtoObjectInfo<{parser.Identifier}>?";
            
            private FieldDeclarationSyntax EmitTypeInfoField()
            {
                return SF.FieldDeclaration(SF.VariableDeclaration(SF.ParseTypeName(_protoTypeInfoNullableFullName)).AddVariables(SF.VariableDeclarator(SF.Identifier("_typeInfo"))))
                    .AddModifiers(SF.Token(SK.PrivateKeyword), SF.Token(SK.StaticKeyword));
            }

            private PropertyDeclarationSyntax EmitTypeInfoProperty()
            {
                return SF.PropertyDeclaration(SF.ParseTypeName(_protoTypeInfoFullName), SF.ParseToken("TypeInfo"))
                    .AddModifiers(SF.Token(SK.PublicKeyword), SF.Token(SK.StaticKeyword))
                    .AddAccessorListAccessors(
                        SF.AccessorDeclaration(SK.GetAccessorDeclaration)
                            .WithExpressionBody(
                                SF.ArrowExpressionClause(
                                    SF.AssignmentExpression(SK.CoalesceAssignmentExpression,
                                        SF.IdentifierName("_typeInfo"),
                                        SF.InvocationExpression(SF.IdentifierName("CreateTypeInfo")).WithArgumentList(SF.ArgumentList())
                                    )
                                )
                            )
                            .WithSemicolonToken(SF.Token(SK.SemicolonToken))
                    );
            }
            
            private MethodDeclarationSyntax EmitTypeInfoCreationMethod()
            {
                var fields = parser.Fields.ToDictionary(
                    kv => kv.Key,
                    kv => SF.ObjectCreationExpression(SF.ParseTypeName($"global::Lagrange.Proto.Serialization.Metadata.ProtoFieldInfo<{kv.Value.TypeSyntax}>"))
                        .WithArgumentList(SF.ArgumentList(
                            SF.SeparatedList<ArgumentSyntax>(
                                [
                                    SF.Argument(SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(kv.Key))),
                                    SF.Argument(SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Serialization.WireType"), SF.IdentifierName(kv.Value.WireType.ToString()))),
                                    SF.Argument(SF.TypeOfExpression(SF.ParseTypeName(parser.Identifier)))
                                ]
                            )
                        ))
                );
                var dictionaryInitialize = SF.ObjectCreationExpression(SF.ParseTypeName("global::System.Collections.Generic.Dictionary<int, global::Lagrange.Proto.Serialization.Metadata.ProtoFieldInfo>"))
                    .WithArgumentList(SF.ArgumentList())
                    .WithInitializer(SF.InitializerExpression(SK.CollectionInitializerExpression).AddExpressions(
                        fields.Select(kv =>
                        {
                            var exprs = new CollectionElementSyntax[] { SF.ExpressionElement(SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(kv.Key))) };
                            var left = SF.CollectionExpression(SF.SeparatedList(exprs));
                            return SF.AssignmentExpression(SK.SimpleAssignmentExpression, left, kv.Value);
                        }).Cast<ExpressionSyntax>().ToArray()
                    ));
                var fieldsAssignment = SF.AssignmentExpression(SK.SimpleAssignmentExpression, SF.IdentifierName("Fields"), dictionaryInitialize);
                var returnStatement = SF.ReturnStatement(SF.ObjectCreationExpression(SF.ParseTypeName(_protoTypeInfoFullName))
                    .WithArgumentList(SF.ArgumentList())
                    .WithInitializer(SF.InitializerExpression(SK.ObjectInitializerExpression).AddExpressions(fieldsAssignment))
                );

                return SF.MethodDeclaration(SF.ParseTypeName(_protoTypeInfoFullName), "CreateTypeInfo")
                    .AddModifiers(SF.Token(SK.PrivateKeyword), SF.Token(SK.StaticKeyword))
                    .WithBody(SF.Block(returnStatement));
            }
        }
    }
}