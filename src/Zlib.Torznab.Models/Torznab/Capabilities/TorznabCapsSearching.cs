using System.Xml.Serialization;

namespace Zlib.Torznab.Models.Torznab.Capabilities;

public class TorznabCapsSearching
{
    [XmlElement(ElementName = "search")]
    public TorznabCapsSearchingParams? Search { get; set; }

    [XmlElement(ElementName = "book-search")]
    public TorznabCapsSearchingParams? BookSearch { get; set; }
}
