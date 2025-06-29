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

public interface IEmptyParameterApiHandler<TResult> : IApiHandler<object, TResult> where TResult : notnull
{
    async Task<TResult> IApiHandler<object, TResult>.HandleAsync(object parameter, CancellationToken token)
    {
        return await HandleAsync(token);
    }

    Task<TResult> HandleAsync(CancellationToken token);
}

public interface IEmptyResultApiHandler<TParameter> : IApiHandler<TParameter, object> where TParameter : notnull
{
    private static readonly object _result = new();

    async Task<object> IApiHandler<TParameter, object>.HandleAsync(TParameter parameter, CancellationToken token)
    {
        await HandleAsync(parameter, token);
        return _result;
    }

    new Task HandleAsync(TParameter parameter, CancellationToken token);
}