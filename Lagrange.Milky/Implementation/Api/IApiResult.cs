namespace Lagrange.Milky.Implementation.Api;

public interface IApiResult
{
    public string Status { get; }

    public long Retcode { get; }

    public static IApiResult Ok<TData>(TData data) => new ApiOkResult<TData>()
    {
        Data = data,
    };

    public static IApiResult Failed(long retcode, string message) => new ApiFailedResult()
    {
        Retcode = retcode,
        Message = message,
    };
}