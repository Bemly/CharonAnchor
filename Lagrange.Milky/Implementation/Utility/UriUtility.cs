namespace Lagrange.Milky.Utility;

public static class UriUtility
{
    private static readonly HttpClient _client = new();

    public static async Task<MemoryStream> ToMemoryStreamAsync(string uri, CancellationToken token)
    {
        return uri[..uri.IndexOf("://")] switch
        {
            "file" => new MemoryStream(File.ReadAllBytes(uri[8..])),
            "http" or "https" => await HttpUriToMemoryStreamAsync(uri, token),
            "base64" => new MemoryStream(Convert.FromBase64String(uri[9..])),
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
        var response = await _client.SendAsync(request, token);
        if (!response.IsSuccessStatusCode) throw new Exception($"Unexpected HTTP status code({response.StatusCode})");

        var output = new MemoryStream();
        await response.Content.CopyToAsync(output, null, token);

        return output;
    }
}