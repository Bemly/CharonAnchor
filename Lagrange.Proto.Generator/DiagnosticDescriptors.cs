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
}