using System.Text.Json.Serialization;

namespace Lagrange.Milky.Implementation.Entity.Segment.Common.Data;

public class XmlData
{
    [JsonPropertyName("service_id")]
    public required int ServiceId { get; init; }

    [JsonPropertyName("xml_payload")]
    public required string XmlPayload { get; init; }
}