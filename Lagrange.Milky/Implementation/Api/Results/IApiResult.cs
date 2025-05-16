namespace Lagrange.Milky.Implementation.Api.Results;

public interface IApiResult
{
    public string Status { get; }

    public long Retcode { get; }
}