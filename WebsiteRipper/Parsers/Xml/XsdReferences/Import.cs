namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceNode(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Import : XmlReference
    {
        public Import(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
