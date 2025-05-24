namespace Lagrange.Milky.Core.Utility.CaptchaResolver;

public interface ICaptchaResolver
{
    Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token = default);
}