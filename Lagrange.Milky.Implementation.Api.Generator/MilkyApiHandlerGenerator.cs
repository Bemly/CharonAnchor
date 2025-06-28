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

    private const string MilkyJsonContextSyntaxName = "JsonContext";
    private const string JsonSerializableSyntaxName = "JsonSerializable";
    private const string IApiHandlerBaseFullName = "Lagrange.Milky.Api.Handler.IApiHandler`2";

    // private void Check(SourceProductionContext context, (ImmutableArray<ApiHandlerInfo> Infos, ImmutableArray<IEnumerable<INamedTypeSymbol>> Symbols) tuple)
    // {
    //     var types = tuple.Symbols.SelectMany(@as => @as).ToArray();

    //     foreach (var info in tuple.Infos)
    //     {
    //         var iApiHandlerGSymbol = info.SemanticModel.Compilation.GetTypeByMetadataName(IApiHandlerGFullName)
    //                     ?? throw new NullReferenceException($"{IApiHandlerGFullName} INamedSymbol not found");
    //         var iApiHandlerSymbol = info.TargetSymbol.AllInterfaces.FirstOrDefault(s => s.OriginalDefinition.SEquals(iApiHandlerGSymbol));
    //         if (iApiHandlerSymbol == null)
    //         {
    //             context.ReportDiagnostic(Diagnostic.Create(
    //                 DiagnosticDescriptors.NotImplementIApiHandler,
    //                 info.TargetNode.GetLocation(),
    //                 info.TargetNode.Identifier.Text
    //             ));
    //             continue;
    //         }

    //         var parameterSymbol = iApiHandlerSymbol.TypeArguments[0];
    //         bool hasParameterSymbol = parameterSymbol.SEquals(info.SemanticModel.Compilation.ObjectType);
    //         if (!hasParameterSymbol)
    //         {
    //             hasParameterSymbol = types.Contains(parameterSymbol, SymbolEqualityComparer.Default);
    //         }
    //         if (!hasParameterSymbol) context.ReportDiagnostic(Diagnostic.Create(
    //             DiagnosticDescriptors.NotUsedJsonSerializable,
    //             info.TargetNode.GetLocation(),
    //             parameterSymbol
    //         ));

    //         var resultSymbol = iApiHandlerSymbol.TypeArguments[1];
    //         bool hasResultSymbol = resultSymbol.SEquals(info.SemanticModel.Compilation.ObjectType);
    //         if (!hasResultSymbol)
    //         {
    //             hasResultSymbol = types.Contains(resultSymbol, SymbolEqualityComparer.Default);
    //         }
    //         if (!hasResultSymbol) context.ReportDiagnostic(Diagnostic.Create(
    //             DiagnosticDescriptors.NotUsedJsonSerializable,
    //             info.TargetNode.GetLocation(),
    //             resultSymbol
    //         ));
    //     }
    // }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var apis = context.SyntaxProvider.ForAttributeWithMetadataName(ApiAttributeFullName, (_, _) => true, ToApiHandlerInfo).Collect();
        context.RegisterSourceOutput(apis, Output);

        var targets = context.SyntaxProvider.CreateSyntaxProvider(JsonSerializableFilter, GetJsonSerializableTarget).Collect();
        var apisAndTargets = apis.Combine(targets);
        context.RegisterSourceOutput(apisAndTargets, Check);
    }

    private void Check(SourceProductionContext context, (ImmutableArray<ApiHandlerInfo> Apis, ImmutableArray<IEnumerable<INamedTypeSymbol>> Targets) apisAndTargets)
    {
        var types = apisAndTargets.Targets.SelectMany(@as => @as).ToArray();

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
        context.AddSource("Lagrange.Milky.Extension.ServiceCollectionExtension.g.cs", $$"""
        namespace Lagrange.Milky.Extension;

        public static partial class ServiceCollectionExtension
        {
            public static partial TServiceCollection AddApiHandlers<TServiceCollection>(this TServiceCollection services) where TServiceCollection : global::Microsoft.Extensions.DependencyInjection.IServiceCollection
            {
        {{string.Join("\n", infos.Select(info => $"        global::Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions.AddKeyedSingleton<global::Lagrange.Milky.Api.Handler.IApiHandler, {info.HandlerTypeFullName}>(services, \"{info.ApiName}\");"))}}

                return services;
            }
        }
        """);
    }

    private IEnumerable<INamedTypeSymbol> GetJsonSerializableTarget(GeneratorSyntaxContext context, CancellationToken token)
    {
        return context.SemanticModel
            .GetDeclaredSymbol(((AttributeSyntax)context.Node).GetAnnotatedClass()!)!
            .GetAttributes()
            .Where(a =>
                a.AttributeClass?.Name == JsonSerializableSyntaxName + "Attribute" &&
                a.AttributeClass?.ContainingNamespace.ToString() == "System.Text.Json.Serialization")
            .Select(a => (INamedTypeSymbol)a.ConstructorArguments[0].Value!);
    }

    private bool JsonSerializableFilter(SyntaxNode node, CancellationToken token)
    {
        return node is AttributeSyntax attribute // is attribute
            && attribute.Name.ToString() == JsonSerializableSyntaxName // is JsonSerializable
            && attribute.GetAnnotatedClass() is ClassDeclarationSyntax @class // is annotate on class
            && @class.Identifier.Text == MilkyJsonContextSyntaxName; // annotate class is JsonContext
    }
}
