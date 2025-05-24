using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;

namespace Lagrange.Milky.Implementation.Api;

public interface IApiHandler
{
    Type ParameterType { get; }

    Task<IApiResult> HandleAsync(IApiParameter parameter, CancellationToken token);
}

public interface IApiHandler<in TParameter> : IApiHandler where TParameter : IApiParameter
{
    Type IApiHandler.ParameterType => typeof(TParameter);

    Task<IApiResult> IApiHandler.HandleAsync(IApiParameter parameter, CancellationToken token)
    {
        return HandleAsync((TParameter)parameter, token);
    }

    Task<IApiResult> HandleAsync(TParameter parameter, CancellationToken token);
}