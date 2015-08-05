namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceNode(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Include : XmlReference
    {
        public Include(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
