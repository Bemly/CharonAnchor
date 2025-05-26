using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Milky.ApiHandler.Generator;

[Generator(LanguageNames.CSharp)]
public class MilkyApiHandlerGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "Lagrange.Milky.Implementation.Api.ApiAttribute";

    private const string InterfaceFullName = "Lagrange.Milky.Implementation.Api.IApiHandler`2";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var result = context.SyntaxProvider.ForAttributeWithMetadataName(AttributeFullName, (_, _) => true, ToApiHandlerInfo);

        context.RegisterSourceOutput(result.Collect(), Output);
    }

    private TransformResult<ApiHandlerInfo> ToApiHandlerInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (context.TargetNode is not ClassDeclarationSyntax classSyntax)
        {
            return new(Diagnostic.Create(DiagnosticDescriptors.NotClass, context.TargetNode.GetLocation()));
        }

        if (context.SemanticModel.GetDeclaredSymbol(classSyntax) is not INamedTypeSymbol classSymbol)
        {
            return new(Diagnostic.Create(
                DiagnosticDescriptors.SymbolNotFound,
                context.TargetNode.GetLocation(),
                classSyntax.Identifier.Text
            ));
        }

        var interfaceGenericSymbol = context.SemanticModel.Compilation.GetTypeByMetadataName(InterfaceFullName);
        if (interfaceGenericSymbol == null)
        {
            return new(Diagnostic.Create(
                DiagnosticDescriptors.SymbolNotFound,
                context.TargetNode.GetLocation(),
                InterfaceFullName
            ));
        }

        var interfaceSymbol = classSymbol.AllInterfaces.FirstOrDefault(symbol => SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition, interfaceGenericSymbol));
        if (interfaceSymbol == null)
        {
            return new(Diagnostic.Create(DiagnosticDescriptors.NotIApiHandler, context.TargetNode.GetLocation()));
        }

        string apiName = (string)context.Attributes[0].ConstructorArguments[0].Value!;
        string handlerTypeFullName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string parameterTypeFullName = interfaceSymbol.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        string resultTypeFullName = interfaceSymbol.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);


        return new TransformResult<ApiHandlerInfo>(new ApiHandlerInfo(apiName, handlerTypeFullName, parameterTypeFullName, resultTypeFullName));
    }

    private void Output(SourceProductionContext context, ImmutableArray<TransformResult<ApiHandlerInfo>> results)
    {
        var infos = new List<ApiHandlerInfo>();

        foreach (var result in results)
        {
            if (result.Result == null)
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else
            {
                infos.Add(result.Result);
            }
        }

        context.AddSource("Lagrange.Milky.Implementation.Extension.ServiceCollectionExtension.g.cs", $$"""
        namespace Lagrange.Milky.Implementation.Extension;

        public static class ServiceCollectionExtension
        {
            public static TServiceCollection AddApiHandlers<TServiceCollection>(this TServiceCollection services) where TServiceCollection : global::Microsoft.Extensions.DependencyInjection.IServiceCollection
            {
        {{string.Join("\n", infos.Select(info => $"        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddKeyedSingleton<global::Lagrange.Milky.Implementation.Api.IApiHandler, {info.HandlerTypeFullName}>(services, \"{info.ApiName}\");"))}}

                return services;
            }
        }
        """);
    }
}
