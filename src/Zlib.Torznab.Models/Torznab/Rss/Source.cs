using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "source")]
public class Source
{
    [XmlAttribute(AttributeName = "url")]
    public string Url { get; set; } = string.Empty;

    [XmlText]
    public string Text { get; set; } = string.Empty;
}
