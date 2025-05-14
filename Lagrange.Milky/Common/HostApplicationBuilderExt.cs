using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky.Common;

public static class HostApplicationBuilderExt
{
    public static HostApplicationBuilder UseJsonFileWithComments(
        this HostApplicationBuilder builder,
        string path)
    {
        string json = File.ReadAllText(path);
        var jsonDocument = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });
        string normalizedJson = jsonDocument.RootElement.ToString();
        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(normalizedJson)));
        return builder;
    }
}