namespace Lagrange.OneBot.Message.Factory;

public static class CommonResolver
{
    private static readonly HttpClient Client = new();
    
    public static Stream? ResolveStream(string url)
    {
        if (url.StartsWith("base64://")) return new MemoryStream(Convert.FromBase64String(url.Replace("base64://", "")));

        Uri uri = new(url);

        return uri.Scheme switch
        {
            "http" or "https" => Client.GetAsync(uri).Result.Content.ReadAsStreamAsync().Result,
            "file" => new FileStream(Path.GetFullPath(uri.LocalPath), FileMode.Open, FileAccess.Read, FileShare.Read),
            _ => null,
        };
    }
}