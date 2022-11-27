using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

[XmlRoot(ElementName = "caps")]
public class TorznabCapabilitiesResponse : TorznabResponseBase
{
    [XmlElement(ElementName = "server")]
    public TorznabServer Server { get; set; } = new();

    [XmlElement(ElementName = "registration")]
    public TorznabRegistration Registration { get; set; } = new();

    [XmlElement(ElementName = "limits")]
    public TorznabLimits Limits { get; set; } = new();

    [XmlElement(ElementName = "searching")]
    public TorznabCapsSearching Searching { get; set; } = new();

    [XmlElement(ElementName = "categories")]
    public TorznabCategories Categories { get; set; } = new();
}
