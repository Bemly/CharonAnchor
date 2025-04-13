using Microsoft.CodeAnalysis;

namespace Lagrange.Proto.Generator;

public static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor MustBePartialClass { get; } = new(
        id: "PROTO001",
        title: "ProtoPackable class {0} must be partial class",
        messageFormat: "ProtoPackable class {0} must be partial class",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor DuplicateFieldNumber { get; } = new(
        id: "PROTO002",
        title: "Duplicate field number {0} in class {1}",
        messageFormat: "Duplicate field number {0} in class {1}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static DiagnosticDescriptor InvalidNumberHandling { get; } = new(
        id: "PROTO003",
        title: "Invalid number handling for field {0} in class {1}",
        messageFormat: "Invalid number handling for field {0} in class {1}",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    
    public static DiagnosticDescriptor NestedTypeMustBeProtoPackable { get; } = new(
        id: "PROTO004",
        title: "Nested type {0} contained in {1} must be ProtoPackable",
        messageFormat: "Nested type {0} contained in {1} must be ProtoPackable",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}