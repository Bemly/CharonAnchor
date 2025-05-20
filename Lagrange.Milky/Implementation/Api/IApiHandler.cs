namespace Lagrange.Milky.Implementation.Api;

public interface IApiHandler
{
    Type ParameterType { get; }

    Task<IApiResult> HandleAsync(object parameter, CancellationToken token);
}

public interface IApiHandler<TParameter> : IApiHandler
{
    Type IApiHandler.ParameterType => typeof(TParameter);

    Task<IApiResult> IApiHandler.HandleAsync(object parameter, CancellationToken token)
    {
        return HandleAsync((TParameter)parameter, token);
    }

    Task<IApiResult> HandleAsync(TParameter parameter, CancellationToken token);
}