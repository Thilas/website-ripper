namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("source", Kind = ReferenceKind.Hyperlink)]
    public sealed class AppInfo : XmlReference
    {
        public AppInfo(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
