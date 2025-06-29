using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Milky.Implementation.Api.Generator;

public class ApiHandlerInfo(ClassDeclarationSyntax targetNode, INamedTypeSymbol targetSymbol, SemanticModel semanticModel, string apiName, bool debug, string handlerTypeFullName)
{
    public ClassDeclarationSyntax TargetNode { get; } = targetNode;

    public INamedTypeSymbol TargetSymbol { get; } = targetSymbol;

    public SemanticModel SemanticModel { get; } = semanticModel;

    public string ApiName { get; } = apiName;

    public bool Debug { get; } = debug;

    public string HandlerTypeFullName { get; } = handlerTypeFullName;
}