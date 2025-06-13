namespace Lagrange.Milky.Api.Handler;

public interface IApiHandler
{
    Type ParameterType { get; }

    Task<object> HandleAsync(object parameter, CancellationToken token);
}

public interface IApiHandler<TParameter, TResult> : IApiHandler where TParameter : notnull where TResult : notnull
{
    Type IApiHandler.ParameterType => typeof(TParameter);

    async Task<object> IApiHandler.HandleAsync(object parameter, CancellationToken token)
    {
        return await HandleAsync((TParameter)parameter, token);
    }

    Task<TResult> HandleAsync(TParameter parameter, CancellationToken token);
}