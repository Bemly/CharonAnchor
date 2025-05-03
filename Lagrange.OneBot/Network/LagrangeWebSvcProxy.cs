namespace Lagrange.OneBot.Network;

/// <summary>
/// A Proxy to LagrangeWebSvcCollection
/// </summary>
public class LagrangeWebSvcProxy
{
    private LagrangeWebSvcCollection _webService = null!;
    
    public void RegisterWebSvc(LagrangeWebSvcCollection webSvc)
    {
        _webService = webSvc;
    }
    
    public Task SendJsonAsync<T>(T json, string? identifier = null, CancellationToken cancellationToken = default) 
        => _webService.SendJsonAsync(json, identifier, cancellationToken);
}