namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceElement(Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("href")]
    public sealed class Import : XmlReference
    {
        public Import(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
