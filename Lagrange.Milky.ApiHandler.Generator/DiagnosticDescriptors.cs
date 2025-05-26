using Microsoft.CodeAnalysis;

namespace Lagrange.Milky.ApiHandler.Generator;

public static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor NotClass { get; } = new(
        id: "MILKYAPI001",
        title: "ApiAttribute can only be applied to classes",
        messageFormat: "ApiAttribute can only be applied to classes",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor SymbolNotFound { get; } = new(
        id: "MILKYAPI002",
        title: "{0} not found",
        messageFormat: "{0} not found",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor NotIApiHandler { get; } = new(
        id: "MILKYAPI003",
        title: "ApiAttribute can only be applied to IApiHandler<in TParameter, out TResult> implementations",
        messageFormat: "ApiAttribute can only be applied to IApiHandler<in TParameter, out TResult> implementations",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor LOGGGGGGG { get; } = new(
        id: "MILKYAPILOGGGGGGG",
        title: "{0}",
        messageFormat: "{0}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}