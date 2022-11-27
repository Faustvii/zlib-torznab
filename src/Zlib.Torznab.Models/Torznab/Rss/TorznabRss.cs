using System.Xml;
using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "rss")]
public class TorznabRss : TorznabResponseBase
{
    [XmlElement(ElementName = "channel")]
    public Channel Channel { get; set; } = new();

    [XmlAttribute(AttributeName = "version")]
    public static string Version => "2.0";
}
