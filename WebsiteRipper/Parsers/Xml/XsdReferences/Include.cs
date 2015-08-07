using System.Xml;

namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Include : XmlReference
    {
        public Include(ReferenceArgs<XmlElement, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
