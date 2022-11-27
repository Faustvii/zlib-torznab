using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "attr", Namespace = "http://torznab.com/schemas/2015/feed")]
public class Attr
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "value")]
    public string Value { get; set; } = string.Empty;
}
