using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

[XmlRoot(ElementName = "registration")]
public class TorznabRegistration
{
    [XmlAttribute(AttributeName = "available")]
    public string Available { get; set; } = "yes";

    [XmlAttribute(AttributeName = "open")]
    public string Open { get; set; } = "yes";
}
