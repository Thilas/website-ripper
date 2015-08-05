namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("source", Kind = ReferenceKind.Hyperlink)]
    public sealed class Documentation : XmlReference
    {
        public Documentation(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
