namespace Lagrange.Milky.Utility.CaptchaResolver;

public interface ICaptchaResolver
{
    Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token = default);
}