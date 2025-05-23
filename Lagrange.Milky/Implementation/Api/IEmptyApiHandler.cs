using Lagrange.Milky.Implementation.Api.Parameter;
using Lagrange.Milky.Implementation.Api.Result;

namespace Lagrange.Milky.Implementation.Api;

public interface IEmptyApiHandler : IApiHandler
{
    Type IApiHandler.ParameterType => typeof(EmptyApiParameter);

    Task<IApiResult> IApiHandler.HandleAsync(IApiParameter parameter, CancellationToken token)
    {
        return HandleAsync(token);
    }

    Task<IApiResult> HandleAsync(CancellationToken token);
}