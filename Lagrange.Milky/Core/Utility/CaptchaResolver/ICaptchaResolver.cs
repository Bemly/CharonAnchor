namespace Lagrange.Milky.Utility;

public interface ICaptchaResolver
{
    Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token = default);
}