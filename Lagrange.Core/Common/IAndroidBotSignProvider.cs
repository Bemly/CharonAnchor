using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Lagrange.Core.Common;

public interface IAndroidBotSignProvider : IBotSignProvider
{
    public Task<byte[]> GetEnergy(long uin, string data);

    public Task<byte[]> GetDebugXwid(long uin, string data);
}

internal class DefaultAndroidBotSignProvider(BotContext context) : IAndroidBotSignProvider, IDisposable
{
    private readonly HttpClient _client = new();

    private readonly string _url = "";
    

    public async Task<SsoSecureInfo?> GetSecSign(long uin, string cmd, int seq, ReadOnlyMemory<byte> body)
    {
        try
        {
            var payload = new JsonObject
            {
                ["uin"] = uin,
                ["command"] = cmd,
                ["seq"] = seq,
                ["buffer"] = Convert.ToHexString(body.Span),
                ["guid"] = Convert.ToHexString(context.Keystore.Guid),
                ["version"] = context.AppInfo.CurrentVersion
            };
            
            var response = await _client.PostAsJsonAsync($"{_url}/sign", payload);
            if (!response.IsSuccessStatusCode) return null;
            
            var content = await response.Content.ReadFromJsonAsync<ResponseRoot<SignResponse>>();
            if (content == null) return null;
            
            return new SsoSecureInfo
            {
                SecSign = Convert.FromHexString(content.Value.Sign),
                SecToken = Convert.FromHexString(content.Value.Token), 
                SecExtra = Convert.FromHexString(content.Value.Extra)
            };
        }
        catch (Exception e)
        {
            // TODO: Log the exception
            return null;
        }
    }

    public async Task<byte[]> GetEnergy(long uin, string data)
    {
        try
        {
            var payload = new JsonObject
            {
                ["uin"] = uin,
                ["data"] = data,
                ["guid"] = Convert.ToHexString(context.Keystore.Guid),
                ["ver"] = context.AppInfo.SdkInfo.SdkVersion,
                ["version"] = context.AppInfo.CurrentVersion
            };
            
            var response = await _client.PostAsJsonAsync($"{_url}/energy", payload);
            if (!response.IsSuccessStatusCode) return [];
            
            var content = await response.Content.ReadFromJsonAsync<ResponseRoot<string>>();
            return content == null ? [] : Convert.FromHexString(content.Value);
        }
        catch (Exception e)
        {
            // TODO: Log the exception
            return [];
        }
    }

    public async Task<byte[]> GetDebugXwid(long uin, string data)
    {
        try
        {
            var payload = new JsonObject
            {
                ["uin"] = uin,
                ["data"] = data,
                ["guid"] = Convert.ToHexString(context.Keystore.Guid),
                ["version"] = context.AppInfo.CurrentVersion
            };
            
            var response = await _client.PostAsJsonAsync($"{_url}/get_tlv553", payload);
            if (!response.IsSuccessStatusCode) return [];
            
            var content = await response.Content.ReadFromJsonAsync<ResponseRoot<string>>();
            return content == null ? [] : Convert.FromHexString(content.Value);
        }
        catch (Exception e)
        {
            // TODO: Log the exception
            return [];
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Serializable]
    private class ResponseRoot<T>
    {
        [JsonPropertyName("value")] public T Value { get; set; } = default!;
    }
    
    [Serializable]
    private class SignResponse
    {
        [JsonPropertyName("sign")] public string Sign { get; set; } = string.Empty;
        
        [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
        
        [JsonPropertyName("extra")] public string Extra { get; set; } = string.Empty;
    }
}