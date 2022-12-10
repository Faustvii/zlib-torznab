using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Zlib.Torznab.Presentation.API.Core;

public class XmlSerializerOutputFormatterTorznabNamespace : XmlSerializerOutputFormatter
{
    protected override void Serialize(
        XmlSerializer xmlSerializer,
        XmlWriter xmlWriter,
        object? value
    )
    {
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("torznab", "http://torznab.com/schemas/2015/feed");
        namespaces.Add("atom", "http://www.w3.org/2005/Atom");
        xmlSerializer.Serialize(xmlWriter, value, namespaces);
    }
}
