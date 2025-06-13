namespace Lagrange.Milky.Utility.CaptchaResolver;

public class ManualCaptchaResolver : ICaptchaResolver
{
    public async Task<(string, string)> ResolveCaptchaAsync(string url, CancellationToken token)
    {
        // Allow interrupt input
        return await Task.Run(() =>
        {
            Console.WriteLine($"Captcha URL: {url}");
            Console.Write("Please enter the ticket: ");
            string ticket = Console.ReadLine() ?? string.Empty;
            Console.Write("Please enter the randstr: ");
            string randstr = Console.ReadLine() ?? string.Empty;

            return (ticket, randstr);
        }, token);
    }
}
