using System.Xml;

namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceElement(Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("href")]
    public sealed class Import : XmlReference
    {
        public Import(ReferenceArgs<XmlElement, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
