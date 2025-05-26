using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Lagrange.Milky.ApiHandler.Generator;

public class TransformResult<T> where T : class
{
    public TransformResult(T? result)
    {
        Diagnostics = [];
        Result = result;
    }

    public TransformResult(params Diagnostic[] diagnostics)
    {
        Diagnostics = diagnostics;
        Result = null;
    }

    public IReadOnlyList<Diagnostic> Diagnostics { get; }

    public T? Result { get; }
}