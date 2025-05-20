namespace Lagrange.Milky.Implementation.Api;

public interface IEmptyApiHandler : IApiHandler
{
    Type IApiHandler.ParameterType => typeof(object);

    Task<IApiResult> IApiHandler.HandleAsync(object parameter, CancellationToken token)
    {
        return HandleAsync(token);
    }

    Task<IApiResult> HandleAsync(CancellationToken token);
}