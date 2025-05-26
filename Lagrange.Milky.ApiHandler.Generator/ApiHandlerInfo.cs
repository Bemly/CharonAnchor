namespace Lagrange.Milky.ApiHandler.Generator;

public class ApiHandlerInfo(string apiName, string handlerTypeFullName, string parameterTypeFullName, string resultTypeFullName)
{
    public string ApiName { get; } = apiName;

    public string HandlerTypeFullName { get; } = handlerTypeFullName;

    public string ParameterTypeFullName { get; } = parameterTypeFullName;

    public string ResultTypeFullName { get; } = resultTypeFullName;
}