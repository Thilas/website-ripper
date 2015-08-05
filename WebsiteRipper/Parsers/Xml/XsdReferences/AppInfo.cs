namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceNode(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("source", Kind = ReferenceKind.Hyperlink)]
    public sealed class AppInfo : XmlReference
    {
        public AppInfo(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
