namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Override : XmlReference
    {
        public Override(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
