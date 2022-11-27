using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "guid")]
public class TorznabGuid
{
    [XmlAttribute(AttributeName = "isPermaLink")]
    public bool IsPermaLink { get; set; }

    [XmlText]
    public string Text { get; set; } = string.Empty;
}
