namespace Lagrange.Milky.Implementation.Apis.Results;

public interface IApiResult
{
    public string Status { get; }

    public long Retcode { get; }
}