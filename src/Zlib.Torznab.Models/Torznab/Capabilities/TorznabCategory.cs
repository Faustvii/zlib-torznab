using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

[XmlRoot(ElementName = "category")]
public class TorznabCategory
{
    [XmlAttribute(AttributeName = "id")]
    public int Id { get; set; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "description")]
    public string Description { get; set; } = string.Empty;

    [XmlElement(ElementName = "subcat")]
    public List<TorznabSubCategory> Subcat { get; set; } = new();
}
