using System.Net;

namespace Lagrange.Milky.Implementation.Extension;

public static class HttpListenerResponseExtension
{
    public static void Send(this HttpListenerResponse response, HttpStatusCode status)
    {
        response.StatusCode = (int)status;
        response.Close();
    }

    public static async Task SendJsonAsync(this HttpListenerResponse response, byte[] body, CancellationToken token)
    {
        response.ContentType = "application/json; charset=utf-8";
        await response.OutputStream.WriteAsync(body, token);
        response.Close();
    }
}