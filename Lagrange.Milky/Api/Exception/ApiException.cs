namespace Lagrange.Milky.Api.Exception;

public class ApiException(long retcode, string error) : System.Exception($"({retcode}) {error}")
{
    public long Retcode { get; } = retcode;

    public string Error { get; } = error;
}