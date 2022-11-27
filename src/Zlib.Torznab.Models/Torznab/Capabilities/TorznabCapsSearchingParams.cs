using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

[XmlRoot(ElementName = "search")]
public class TorznabCapsSearchingParams
{
    [XmlAttribute(AttributeName = "available")]
    public string Available { get; set; } = "yes";

    [XmlAttribute(AttributeName = "supportedParams")]
    public string SupportedParams { get; set; } = "q";
}
