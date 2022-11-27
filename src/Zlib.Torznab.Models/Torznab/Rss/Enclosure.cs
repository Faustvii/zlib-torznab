using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "enclosure")]
public class Enclosure
{
    [XmlAttribute(AttributeName = "url")]
    public string Url { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "type")]
    public string Type { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "length")]
    public int Length { get; set; }
}
