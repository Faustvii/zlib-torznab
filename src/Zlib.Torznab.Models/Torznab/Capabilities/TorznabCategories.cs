using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

[XmlRoot(ElementName = "categories")]
public class TorznabCategories
{
    [XmlElement(ElementName = "category")]
    public List<TorznabCategory> Category { get; set; } = new();
}
