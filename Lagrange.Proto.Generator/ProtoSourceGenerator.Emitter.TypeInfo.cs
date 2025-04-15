using Lagrange.Proto.Serialization;
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
            
            private const string ProtoNumberHandlingFullName = "global::Lagrange.Proto.Serialization.ProtoNumberHandling";
            
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
                    kv => (kv.Key << 3) | (byte)kv.Value.WireType,
                    kv =>
                    {
                        string first = kv.Value.WireType switch
                        {
                            WireType.Fixed32 or WireType.Fixed64 => $"{ProtoNumberHandlingFullName}.{kv.Value.WireType}",
                            _ => $"{ProtoNumberHandlingFullName}.Default"
                        };
                        string numberHandling = $"{first}{(kv.Value.IsSigned ? $" | {ProtoNumberHandlingFullName}.Signed" : "")}";
                        
                        return SF.ObjectCreationExpression(SF.ParseTypeName($"global::Lagrange.Proto.Serialization.Metadata.ProtoFieldInfo<{kv.Value.TypeSyntax}>"))
                            .WithArgumentList(SF.ArgumentList(
                                SF.SeparatedList<ArgumentSyntax>(
                                    [
                                        SF.Argument(SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(kv.Key))),
                                        SF.Argument(SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Serialization.WireType"), SF.IdentifierName(kv.Value.WireType.ToString()))),
                                        SF.Argument(SF.TypeOfExpression(SF.ParseTypeName(parser.Identifier)))
                                    ]
                                )
                            ))
                            .WithInitializer(SF.InitializerExpression(SK.ObjectInitializerExpression).AddExpressions(
                                    SF.AssignmentExpression(SK.SimpleAssignmentExpression, SF.IdentifierName("Get"), EmitTypeInfoGetter(kv.Value.Name)),
                                    SF.AssignmentExpression(SK.SimpleAssignmentExpression, SF.IdentifierName("Set"), EmitTypeInfoSetter(kv.Value.Name)),
                                    SF.AssignmentExpression(SK.SimpleAssignmentExpression, SF.IdentifierName("NumberHandling"), SF.IdentifierName(numberHandling))
                                )
                            );
                    });
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
                
                var lambda = SF.ParenthesizedLambdaExpression(SF.ParameterList(), SF.ObjectCreationExpression(SF.ParseTypeName(parser.Identifier)).WithArgumentList(SF.ArgumentList()));
                var objectCreatorAssignment = SF.AssignmentExpression(SK.SimpleAssignmentExpression, SF.IdentifierName("ObjectCreator"), lambda);
                var ignoreDefaultAssignment = SF.AssignmentExpression(SK.SimpleAssignmentExpression, SF.IdentifierName("IgnoreDefaultFields"), SF.LiteralExpression(parser.IgnoreDefaultFields ? SK.TrueLiteralExpression : SK.FalseLiteralExpression));
                
                var returnStatement = SF.ReturnStatement(SF.ObjectCreationExpression(SF.ParseTypeName(_protoTypeInfoFullName))
                    .WithArgumentList(SF.ArgumentList())
                    .WithInitializer(SF.InitializerExpression(SK.ObjectInitializerExpression).AddExpressions(fieldsAssignment, objectCreatorAssignment, ignoreDefaultAssignment))
                );

                return SF.MethodDeclaration(SF.ParseTypeName(_protoTypeInfoFullName), "CreateTypeInfo")
                    .AddModifiers(SF.Token(SK.PrivateKeyword), SF.Token(SK.StaticKeyword))
                    .WithBody(SF.Block(returnStatement));
            }
            
            private ExpressionSyntax EmitTypeInfoGetter(string prop)
            {
                var cast = SF.CastExpression(SF.ParseTypeName(parser.Identifier), SF.IdentifierName("obj"));
                var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.ParenthesizedExpression(cast), SF.IdentifierName(prop));

                return SF.SimpleLambdaExpression(SF.Parameter(SF.Identifier("obj")), access);
            }
        
            private ExpressionSyntax EmitTypeInfoSetter(string prop)
            {
                var parameters = SF.ParameterList().AddParameters(SF.Parameter(SF.Identifier("obj")), SF.Parameter(SF.Identifier("value")));
                var cast = SF.CastExpression(SF.ParseTypeName(parser.Identifier), SF.IdentifierName("obj"));
                var left = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.ParenthesizedExpression(cast), SF.IdentifierName(prop));
                var right = SF.IdentifierName("value");
            
                return SF.ParenthesizedLambdaExpression(parameters,
                    SF.AssignmentExpression(SK.SimpleAssignmentExpression, left, right)
                );
            }
        }
    }
}