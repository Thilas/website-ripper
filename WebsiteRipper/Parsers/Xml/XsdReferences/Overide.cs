using System.Xml;

namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Override : XmlReference
    {
        public Override(ReferenceArgs<XmlElement, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
