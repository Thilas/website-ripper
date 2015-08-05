namespace WebsiteRipper.Parsers.Xml.XsltReferences
{
    [ReferenceNode(Namespace = XmlParser.XsltNamespace)]
    [ReferenceAttribute("href")]
    public sealed class Import : XmlReference
    {
        public Import(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
