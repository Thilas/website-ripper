namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceElement(Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("href")]
    public sealed class Include : XmlReference
    {
        public Include(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
