using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Lagrange.Milky.Implementation.Api.Generator.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Milky.Implementation.Api.Generator;

[Generator(LanguageNames.CSharp)]
public class MilkyApiHandlerGenerator : IIncrementalGenerator
{
    private const string ApiAttributeFullName = "Lagrange.Milky.Api.ApiAttribute";
    private const string ApiAttributeNameName = "Name";
    private const string ApiAttributeDebugName = "Debug";

    private const string MilkyJsonContextSyntaxName = "JsonContext";
    private const string JsonSerializableSyntaxName = "JsonSerializable";

    private const string IApiHandlerBaseFullName = "Lagrange.Milky.Api.Handler.IApiHandler`2";
    private const string IEmptyParameterApiHandlerBaseFullName = "Lagrange.Milky.Api.Handler.IEmptyParameterApiHandler`1";
    private const string IEmptyResultApiHandlerBaseFullName = "Lagrange.Milky.Api.Handler.IEmptyResultApiHandler`1";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var apis = context.SyntaxProvider.ForAttributeWithMetadataName(ApiAttributeFullName, (_, _) => true, ToApiHandlerInfo).Collect();
        context.RegisterSourceOutput(apis, Output);

        var targets = context.SyntaxProvider.CreateSyntaxProvider(JsonSerializableFilter, GetJsonSerializableTarget).Collect();
        var apisAndTargets = apis.Combine(targets);
        context.RegisterSourceOutput(apisAndTargets, Check);
    }

    private void Check(SourceProductionContext context, (ImmutableArray<ApiHandlerInfo> Apis, ImmutableArray<INamedTypeSymbol> Targets) apisAndTargets)
    {
        var types = apisAndTargets.Targets;

        foreach (var info in apisAndTargets.Apis)
        {
            var iApiHandlerGSymbol = info.SemanticModel.Compilation.GetTypeByMetadataName(IApiHandlerBaseFullName);
            var iApiHandlerSymbol = info.TargetSymbol.AllInterfaces.FirstOrDefault(s => s.OriginalDefinition.SEquals(iApiHandlerGSymbol));
            if (iApiHandlerSymbol == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NotImplementIApiHandler,
                    info.TargetNode.GetLocation(),
                    info.TargetNode.Identifier.Text
                ));
                continue;
            }

            var parameterSymbol = iApiHandlerSymbol.TypeArguments[0];
            var parameterIsObject = parameterSymbol.SEquals(info.SemanticModel.Compilation.ObjectType);
            if (parameterIsObject)
            {
                var compilation = info.SemanticModel.Compilation;
                var iEmptyParameterApiHandlerGSymbol = compilation.GetTypeByMetadataName(IEmptyParameterApiHandlerBaseFullName);
                var interfaces = info.TargetSymbol.AllInterfaces;
                if (interfaces.FirstOrDefault(s => s.OriginalDefinition.SEquals(iEmptyParameterApiHandlerGSymbol)) == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.NotImplementIEmptyParameterApiHandler,
                        info.TargetNode.GetLocation(),
                        info.TargetNode.Identifier.Text
                    ));
                }
            }
            bool hasParameterSymbol = parameterSymbol.SEquals(info.SemanticModel.Compilation.ObjectType);
            if (!hasParameterSymbol)
            {
                hasParameterSymbol = types.Contains(parameterSymbol, SymbolEqualityComparer.Default);
            }
            if (!hasParameterSymbol) context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NotUsedJsonSerializable,
                info.TargetNode.GetLocation(),
                parameterSymbol
            ));

            var resultSymbol = iApiHandlerSymbol.TypeArguments[1];
            var resultIsObject = resultSymbol.SEquals(info.SemanticModel.Compilation.ObjectType);
            if (resultIsObject)
            {
                var compilation = info.SemanticModel.Compilation;
                var iEmptyResultApiHandlerGSymbol = compilation.GetTypeByMetadataName(IEmptyResultApiHandlerBaseFullName);
                var interfaces = info.TargetSymbol.AllInterfaces;
                if (interfaces.FirstOrDefault(s => s.OriginalDefinition.SEquals(iEmptyResultApiHandlerGSymbol)) == null)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.NotImplementIEmptyResultApiHandler,
                        info.TargetNode.GetLocation(),
                        info.TargetNode.Identifier.Text
                    ));
                }
            }
            bool hasResultSymbol = resultIsObject || types.Contains(resultSymbol, SymbolEqualityComparer.Default);
            if (!hasResultSymbol) context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NotUsedJsonSerializable,
                info.TargetNode.GetLocation(),
                resultSymbol
            ));
        }
    }

    private ApiHandlerInfo ToApiHandlerInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        int length = context.Attributes[0].ConstructorArguments.Length;

        return new ApiHandlerInfo(
            (ClassDeclarationSyntax)context.TargetNode,
            (INamedTypeSymbol)context.TargetSymbol,
            context.SemanticModel,
            (string)context.Attributes[0].ConstructorArguments[0].Value!,
            (bool)(length == 2 ? context.Attributes[0].ConstructorArguments[1].Value! : false),
            context.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        );
    }

    private void Output(SourceProductionContext context, ImmutableArray<ApiHandlerInfo> infos)
    {
        context.AddSource("Lagrange.Milky.Extension.ServiceCollectionExtension.g.cs", $$"""
        namespace Lagrange.Milky.Extension;

        public static partial class ServiceCollectionExtension
        {
            static partial void AddApiHandlersInternal<TServiceCollection>(TServiceCollection services, bool debug) where TServiceCollection : global::Microsoft.Extensions.DependencyInjection.IServiceCollection
            {
        {{string.Join("\n", infos.Select(info => $"        {(info.Debug ? "if (debug) " : "")}global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddKeyedSingleton<global::Lagrange.Milky.Api.Handler.IApiHandler, {info.HandlerTypeFullName}>(services, \"{info.ApiName}\");"))}}
            }
        }
        """);
    }

    private INamedTypeSymbol GetJsonSerializableTarget(GeneratorSyntaxContext context, CancellationToken token)
    {
        var argument = ((AttributeSyntax)context.Node).ArgumentList!.Arguments.First();
        var type = ((TypeOfExpressionSyntax)argument.Expression).Type;
        return (INamedTypeSymbol)context.SemanticModel.GetSymbolInfo(type).Symbol!;
    }

    private bool JsonSerializableFilter(SyntaxNode node, CancellationToken token)
    {
        return node is AttributeSyntax attribute // is attribute
            && attribute.Name.ToString() == JsonSerializableSyntaxName // is JsonSerializable
            && attribute.GetAnnotatedClass() is ClassDeclarationSyntax @class // is annotate on class
            && @class.Identifier.Text == MilkyJsonContextSyntaxName; // annotate class is JsonContext
    }
}
