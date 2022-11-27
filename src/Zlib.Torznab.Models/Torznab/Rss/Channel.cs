using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Rss;

[XmlRoot(ElementName = "channel")]
public class Channel
{
    [XmlElement(ElementName = "link", Namespace = "http://www.w3.org/2005/Atom")]
    public List<Link> Link { get; } = new();

    [XmlElement(ElementName = "title")]
    public string Title { get; } = "Shadow Library";

    [XmlElement(ElementName = "description")]
    public string Description { get; } = "Latest releases";

    [XmlElement(ElementName = "language")]
    public string Language { get; } = "en-gb";

    [XmlElement(ElementName = "ttl")]
    public int Ttl { get; } = 5;

    [XmlElement(ElementName = "lastBuildDate")]
    public DateTime LastBuildDate { get; set; }

    [XmlElement(ElementName = "response")]
    public Response Response { get; set; } = new();

    [XmlElement(ElementName = "item")]
    public List<Item> Item { get; set; } = new();
}
