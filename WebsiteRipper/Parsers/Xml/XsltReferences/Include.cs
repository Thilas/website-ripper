using System.Xml;

namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceElement(Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("href")]
    public sealed class Include : XmlReference
    {
        public Include(ReferenceArgs<XmlElement, XmlAttribute> referenceArgs) : base(referenceArgs) { }
    }
}
