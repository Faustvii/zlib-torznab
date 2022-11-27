using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

[XmlRoot(ElementName = "server")]
public class TorznabServer
{
    [XmlAttribute(AttributeName = "version")]
    public string Version { get; set; } = "1.0";

    [XmlAttribute(AttributeName = "title")]
    public string Title { get; set; } = "Shadow Library";

    [XmlAttribute(AttributeName = "strapline")]
    public string? Strapline { get; set; }

    [XmlAttribute(AttributeName = "email")]
    public string? Email { get; set; }

    [XmlAttribute(AttributeName = "url")]
    public string? Url { get; set; }

    [XmlAttribute(AttributeName = "image")]
    public string? Image { get; set; }
}
