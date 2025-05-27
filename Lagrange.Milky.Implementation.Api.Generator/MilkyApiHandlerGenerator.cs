using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Lagrange.Milky.Implementation.Api.Generator.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lagrange.Milky.ApiHandler.Generator;

[Generator(LanguageNames.CSharp)]
public class MilkyApiHandlerGenerator : IIncrementalGenerator
{
    private const string ApiFullName = "Lagrange.Milky.Implementation.Api.ApiAttribute";

    private const string IApiHandlerGFullName = "Lagrange.Milky.Implementation.Api.IApiHandler`2";

    private const string MilkyJsonContextSyntaxName = "MilkyJsonContext";
    private const string MilkyJsonContextFullName = "Lagrange.Milky.Implementation.Utility.MilkyJsonUtility+MilkyJsonContext";
    private const string JsonSerializableSyntaxName = "JsonSerializable";
    private const string JsonSerializableFullName = "System.Text.Json.Serialization.JsonSerializableAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var jsonSerializableResult = context.SyntaxProvider.CreateSyntaxProvider(MilkyJsonContextJsonSerializableFilter, ToJsonSerializableValue);
        var apiResult = context.SyntaxProvider.ForAttributeWithMetadataName(ApiFullName, (_, _) => true, ToApiHandlerInfo).Collect();

        context.RegisterSourceOutput(apiResult, Output);
        var provider = apiResult.Combine(jsonSerializableResult.Collect());

        context.RegisterSourceOutput(provider, Check);
    }

    private IEnumerable<INamedTypeSymbol> ToJsonSerializableValue(GeneratorSyntaxContext context, CancellationToken token)
    {
        var model = context.SemanticModel;

        var attributeMethodSymbolInfo = model.GetSymbolInfo(context.Node);
        if (attributeMethodSymbolInfo.Symbol is not IMethodSymbol attributeMethodSymbol) return [];
        var attributeSymbol = attributeMethodSymbol.ContainingType;
        if (!attributeSymbol.SEquals(model.Compilation.GetTypeByMetadataName(JsonSerializableFullName)))
        {
            return [];
        }

        var classSyntax = ((AttributeSyntax)context.Node).GetAnnotatedClass();
        if (classSyntax == null) return [];
        var classSymbol = model.GetDeclaredSymbol(classSyntax);
        if (classSymbol == null) return [];
        if (!classSymbol.SEquals(model.Compilation.GetTypeByMetadataName(MilkyJsonContextFullName)))
        {
            return [];
        }

        return classSymbol.GetAttributes().Select(a => (INamedTypeSymbol)a.ConstructorArguments[0].Value!);
    }

    private bool MilkyJsonContextJsonSerializableFilter(SyntaxNode node, CancellationToken token)
    {
        return node is AttributeSyntax attribute
            && attribute.Name.ToString() == JsonSerializableSyntaxName
            && attribute.GetAnnotatedClass() is ClassDeclarationSyntax @class
            && @class.Identifier.Text == MilkyJsonContextSyntaxName;
    }

    private ApiHandlerInfo ToApiHandlerInfo(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        return new ApiHandlerInfo(
            (ClassDeclarationSyntax)context.TargetNode,
            (INamedTypeSymbol)context.TargetSymbol,
            context.SemanticModel,
            (string)context.Attributes[0].ConstructorArguments[0].Value!,
            context.TargetSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
        );
    }

    private void Output(SourceProductionContext context, ImmutableArray<ApiHandlerInfo> infos)
    {
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

    private void Check(SourceProductionContext context, (ImmutableArray<ApiHandlerInfo> Infos, ImmutableArray<IEnumerable<INamedTypeSymbol>> Symbols) tuple)
    {
        var types = tuple.Symbols.SelectMany(@as => @as).ToArray();

        foreach (var info in tuple.Infos)
        {
            var iApiHandlerGSymbol = info.SemanticModel.Compilation.GetTypeByMetadataName(IApiHandlerGFullName)
                        ?? throw new NullReferenceException($"{IApiHandlerGFullName} INamedSymbol not found");
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
            bool hasResultSymbol = resultSymbol.SEquals(info.SemanticModel.Compilation.ObjectType);
            if (!hasResultSymbol)
            {
                hasResultSymbol = types.Contains(resultSymbol, SymbolEqualityComparer.Default);
            }
            if (!hasResultSymbol) context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.NotUsedJsonSerializable,
                info.TargetNode.GetLocation(),
                resultSymbol
            ));
        }
    }
}
