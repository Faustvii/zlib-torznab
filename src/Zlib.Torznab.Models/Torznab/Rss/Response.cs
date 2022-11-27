using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "response")]
public class Response
{
    [XmlAttribute(AttributeName = "offset")]
    public int Offset { get; set; }
}
