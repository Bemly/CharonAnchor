namespace Lagrange.Milky.Implementation.Utility;

public static class UriUtility
{
    private static readonly HttpClient Client = new();

    public static async Task<MemoryStream> ToMemoryStreamAsync(string uri, CancellationToken token)
    {
        return uri[..uri.IndexOf("://", StringComparison.Ordinal)] switch
        {
            "base64" => new MemoryStream(Convert.FromBase64String(uri[9..])),
            "file" => new MemoryStream(await File.ReadAllBytesAsync(new Uri(uri).LocalPath, token)),
            "http" or "https" => await HttpUriToMemoryStreamAsync(uri, token),
            _ => throw new NotSupportedException(),
        };
    }

    private static async Task<MemoryStream> HttpUriToMemoryStreamAsync(string url, CancellationToken token)
    {
        using var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
        };
        var response = await Client.SendAsync(request, token);
        if (!response.IsSuccessStatusCode) throw new Exception($"Unexpected HTTP status code({response.StatusCode})");

        var output = new MemoryStream();
        await response.Content.CopyToAsync(output, null, token);

        return output;
    }
}