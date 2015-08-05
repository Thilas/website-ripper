namespace WebsiteRipper.Parsers.Xml.XsdReferences
{
    [ReferenceElement(Namespace = XmlParser.XsdNamespace)]
    [ReferenceAttribute("schemaLocation")]
    public sealed class Import : XmlReference
    {
        public Import(XmlReferenceArgs xmlReferenceArgs) : base(xmlReferenceArgs) { }
    }
}
