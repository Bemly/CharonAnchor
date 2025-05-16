using System.Net;

namespace Lagrange.Milky.Implementation.Extensions;

public static class HttpListenerResponseExtension
{
    public static void SendInternalServerError(this HttpListenerResponse response)
    {
        response.StatusCode = 500;
        response.Close();
    }

    public static void SendBadRequest(this HttpListenerResponse response)
    {
        response.StatusCode = 400;
        response.Close();
    }

    public static void SendForbidden(this HttpListenerResponse response)
    {
        response.StatusCode = 403;
        response.Close();
    }

    public static void SendNotFound(this HttpListenerResponse response)
    {
        response.StatusCode = 404;
        response.Close();
    }

    public static void SendUnsupportedMediaType(this HttpListenerResponse response)
    {
        response.StatusCode = 415;
        response.Close();
    }
}