using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lagrange.Milky.Extensions;

public static partial class HostApplicationBuilderExtension
{
    public static HostApplicationBuilder ConfigureConfiguration(this HostApplicationBuilder builder, string path)
    {
        builder.Configuration.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(File.ReadAllText(path))));
        return builder;
    }
}