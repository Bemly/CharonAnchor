using System.Text.Json.Serialization;

namespace Lagrange.OneBot.Entity.Action;

[Serializable]
public class OneBotQrCodeRequest
{
    [JsonPropertyName("k")] public string? K { get; set; }
    
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    
    [JsonPropertyName("confirm")] public bool Confirm { get; set; }
}