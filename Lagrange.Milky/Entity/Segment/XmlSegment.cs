using System.Text.Json.Serialization;

namespace Lagrange.Milky.Entity.Segment;

[method: JsonConstructor]
public class XmlIncomingSegment(XmlSegmentData data) : IncomingSegmentBase<XmlSegmentData>(data)
{
    public XmlIncomingSegment(string serviceId, string xmlPayload) : this(new XmlSegmentData(serviceId, xmlPayload)) { }
}

public class XmlSegmentData(string serviceId, string xmlPayload)
{
    [JsonPropertyName("service_id")]
    public string ServiceId { get; } = serviceId;

    [JsonPropertyName("xml_payload")]
    public string XmlPayload { get; } = xmlPayload;
}