using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

[XmlRoot(ElementName = "limits")]
public class TorznabLimits
{
    [XmlAttribute(AttributeName = "max")]
    public int Max { get; set; } = 100;

    [XmlAttribute(AttributeName = "default")]
    public int Default { get; set; } = 50;
}
