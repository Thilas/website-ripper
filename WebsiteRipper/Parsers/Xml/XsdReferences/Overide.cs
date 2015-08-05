namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceNode(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Override : XmlReference
    {
        public Override(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
