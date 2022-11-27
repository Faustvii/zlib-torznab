using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "item")]
public class Item
{
    [XmlElement(ElementName = "title")]
    public string Title { get; set; } = string.Empty;

    [XmlElement(ElementName = "guid")]
    public TorznabGuid TorznabGuid { get; set; } = new();

    [XmlElement(ElementName = "pubDate")]
    public string PubDate { get; set; } = string.Empty;

    [XmlElement(ElementName = "enclosure")]
    public List<Enclosure> Enclosure { get; set; } = new();

    [XmlElement(ElementName = "source")]
    public Source? Source { get; set; }

    [XmlElement(ElementName = "attr", Namespace = "http://torznab.com/schemas/2015/feed")]
    public List<Attr> Attr { get; set; } = new();
}
