namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceNode(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("source", Kind = ReferenceKind.Hyperlink)]
    public sealed class Documentation : XmlReference
    {
        public Documentation(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
