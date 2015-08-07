using System.Xml;

namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Import : XmlReference
    {
        public Import(ReferenceArgs<XmlElement, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
