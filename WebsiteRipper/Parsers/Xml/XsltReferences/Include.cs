namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceNode(Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("href")]
    public sealed class Include : XmlReference
    {
        public Include(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
